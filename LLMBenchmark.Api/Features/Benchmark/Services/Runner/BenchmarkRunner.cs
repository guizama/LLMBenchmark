using LLMBenchmark.Api.Features.Benchmark.Contracts.Provider;
using LLMBenchmark.Api.Features.Benchmark.Contracts.Validator;
using LLMBenchmark.Api.Features.Benchmark.Enums;
using LLMBenchmark.Api.Features.Benchmark.Helpers;
using LLMBenchmark.Api.Features.Benchmark.Models.Providers;
using LLMBenchmark.Api.Features.Benchmark.Services.Scenarios;
using LLMBenchmark.Api.Features.Benchmark.Validators.Input;
using LLMBenchmark.Api.Features.Benchmark.Validators.Output.Judge.Services;
using LLMBenchmark.Api.Persistence;

namespace LLMBenchmark.Api.Features.Benchmark.Services.Runner;

public sealed class BenchmarkRunner(
    ScenarioLoader scenarioLoader,
    IEnumerable<ILLMProvider> providers,
    AppDbContext dbContext,
    IEnumerable<IBenchmarkValidator> validators,
    JudgeDecisionService judgeDecisionService,
    ScenarioRequestValidator scenarioRequestValidator)
{
    private readonly ScenarioLoader _scenarioLoader = scenarioLoader;
    private readonly IEnumerable<ILLMProvider> _providers = providers;
    private readonly AppDbContext _dbContext = dbContext;
    private readonly IEnumerable<IBenchmarkValidator> _validators = validators;
    private readonly JudgeDecisionService _judgeDecisionService = judgeDecisionService;
    private readonly ScenarioRequestValidator _scenarioRequestValidator = scenarioRequestValidator;

    public async Task<BenchmarkRun> RunAsync(BenchmarkProvider? benchmarkProvider = null, ScenariosLoad scenariosLoad = ScenariosLoad.Simple, CancellationToken cancellationToken = default)
    {
        var selectedProviders = benchmarkProvider is null
                                ? _providers
                                : _providers.Where(x => x.BenchmarkProviderType == benchmarkProvider);

        var scenarios = scenariosLoad == ScenariosLoad.Full
                        ? await _scenarioLoader.FullLoadAsync()
                        : await _scenarioLoader.LoadAsync();

        var run = new BenchmarkRun
        {
            Id = Guid.NewGuid(),
            StartedAtUtc = DateTime.UtcNow,
            TotalScenarios = scenarios.Count
        };

        _dbContext.BenchmarkRuns.Add(run);

        await _dbContext.SaveChangesAsync(cancellationToken);
        try
        {
            foreach (var providerInstance in selectedProviders)
            {
                foreach (var scenario in scenarios)
                {
                    #region ValidateInput
                    var requestValidation = _scenarioRequestValidator.Validate(scenario);

                    if (!requestValidation.IsValid)
                    {
                        var result = new BenchmarkResult
                        {
                            Id = Guid.NewGuid(),
                            BenchmarkRunId = run.Id,
                            Timestamp = DateTime.UtcNow,
                            ScenarioId = scenario.Id ?? string.Empty,

                            Action = scenario.Action ?? string.Empty,
                            Language = scenario.Input.Language ?? string.Empty,

                            Success = false,
                            Error = string.Join(" | ", requestValidation.Errors)
                        };

                        _dbContext.BenchmarkResults.Add(result);

                        run.TotalExecutions++;
                        run.FailureCount++;

                        continue;
                    }

                    #endregion

                    _ = SmsActionParser.TryParse(scenario.Action, out SmsAction action);
                    var request = new LLMRequest
                    {
                        UserText = scenario.Input.Prompt ?? scenario.Input.InputText ?? string.Empty,
                        Action = action,
                        Tone = MapTone(scenario.Input.Tone),
                        Language = MapLanguage(scenario.Input.Language),
                        Creativity = SmsCreativity.Low
                    };

                    var responses = await providerInstance.ExecuteAsync(request, cancellationToken);

                    foreach (var response in responses)
                    {
                        var result = new BenchmarkResult
                        {
                            Id = Guid.NewGuid(),
                            BenchmarkRunId = run.Id,
                            Timestamp = DateTime.UtcNow,
                            ScenarioId = scenario.Id ?? string.Empty,
                            Provider = response.Provider ?? string.Empty,
                            Model = response.Model ?? string.Empty,
                            Action = scenario.Action ?? string.Empty,
                            Language = scenario.Input.Language ?? string.Empty,
                            InputPrompt = request.UserText ?? string.Empty,
                            Output = response.Output ?? string.Empty,
                            PredictedInputTokens = response.InputPrediction?.PredictedInputTokens ?? 0,
                            InputTokens = response.Tokens.InputTokens,
                            OutputTokens = response.Tokens.OutputTokens,
                            EstimatedCost = 0,
                            EndToEndLatencyMs = response.Latency.EndToEndLatencyMs,
                            ProviderLatencyMs = response.Latency.ProviderLatencyMs,
                            OutputCharacters = response.Output?.Length ?? 0,
                            Success = response.Success,
                            Error = response.Error,
                            RawResponse = response.RawResponse,
                            Temperature = response.Temperature,
                            TokenEstimator = response.InputPrediction?.TokenEstimator,
                            InputTokenDelta = response.InputPrediction?.InputTokenDelta,
                            InputTokenErrorPercent = response.InputPrediction?.InputTokenErrorPercent,
                            SystemPrompt = response.SystemPrompt ?? string.Empty,
                            OutputEstimatedSmsSegmentsQtd = SmsSegmentCalculator.Calculate(response.Output ?? string.Empty),
                        };

                        _dbContext.BenchmarkResults.Add(result);

                        run.TotalExecutions++;
                        if (response.Success)
                            run.SuccessCount++;
                        else
                            run.FailureCount++;

                        #region ValidateOutput

                        var deterministicValidators = _validators.Where(x => x.ValidationType != ValidatorType.LlmJudge);
                        var judgeValidators = _validators.Where(x => x.ValidationType == ValidatorType.LlmJudge);

                        foreach (var validator in deterministicValidators)
                        {
                            var validationResult = await validator.ValidateAsync(scenario, result, cancellationToken);
                            result.Validations.Add(validationResult);
                            _dbContext.BenchmarkValidationResults.Add(validationResult);
                        }

                        var judgeDecision = _judgeDecisionService.Evaluate(scenario, result);
                        if (judgeDecision.ShouldRunJudge)
                        {
                            foreach (var validator in judgeValidators)
                            {
                                try
                                {
                                    var validationResult = await validator.ValidateAsync(scenario, result, cancellationToken);
                                    result.Validations.Add(validationResult);
                                    _dbContext.BenchmarkValidationResults.Add(validationResult);
                                }
                                catch (Exception ex)
                                {
                                    result.Validations.Add(new BenchmarkValidationResult
                                    {
                                        Id = Guid.NewGuid(),
                                        BenchmarkResultId = result.Id,

                                        Validator = validator.Name,
                                        ValidationType = validator.ValidationType,

                                        Passed = false,
                                        Score = 0,

                                        Details = $"Validator failed: {ex.Message}"
                                    });
                                }
                            }
                        }

                        #endregion

                        await _dbContext.SaveChangesAsync(cancellationToken);
                    }
                }
            }

            await _dbContext.SaveChangesAsync(CancellationToken.None);
        }
        catch (OperationCanceledException ex)
        {
            run.Status = "Cancelled";
            run.Error = ex.Message;
        }
        catch (Exception ex)
        {
            run.Status = "Failed";
            run.Error = ex.ToString();
        }
        finally
        {
            run.FinishedAtUtc = DateTime.UtcNow;
            if (string.IsNullOrWhiteSpace(run.Status))
                run.Status = "Completed";

            await _dbContext.SaveChangesAsync(CancellationToken.None);
        }

        return run;
    }

    private static SmsTone MapTone(string? tone)
    {
        if (string.IsNullOrWhiteSpace(tone))
            return SmsTone.Neutral;

        return tone.Trim().ToLowerInvariant() switch
        {
            "casual" => SmsTone.Casual,
            "formal" => SmsTone.Formal,
            _ => SmsTone.Neutral
        };
    }

    private static SmsLanguage MapLanguage(
        string? language)
    {
        if (string.IsNullOrWhiteSpace(language))
            return SmsLanguage.PtPT;

        return language.Trim().ToUpperInvariant() switch
        {
            "EN-US" => SmsLanguage.EnUS,
            _ => SmsLanguage.PtPT
        };
    }
}