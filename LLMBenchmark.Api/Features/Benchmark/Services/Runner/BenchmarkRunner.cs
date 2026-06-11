using LLMBenchmark.Api.Features.Benchmark.Contracts;
using LLMBenchmark.Api.Features.Benchmark.Enums;
using LLMBenchmark.Api.Features.Benchmark.Models.Benchmark;
using LLMBenchmark.Api.Features.Benchmark.Models.Providers;
using LLMBenchmark.Api.Features.Benchmark.Services.Scenarios;
using LLMBenchmark.Api.Features.Benchmark.Validators.Contracts;
using LLMBenchmark.Api.Features.Benchmark.Validators.Judge.Services;
using LLMBenchmark.Api.Persistence;

namespace LLMBenchmark.Api.Features.Benchmark.Services.Runner;

public sealed class BenchmarkRunner(
    ScenarioLoader scenarioLoader,
    ILLMProvider provider,
    AppDbContext dbContext,
    IEnumerable<IBenchmarkValidator> validators,
    JudgeDecisionService judgeDecisionService)
{
    private readonly ScenarioLoader _scenarioLoader = scenarioLoader;
    private readonly ILLMProvider _provider = provider;
    private readonly AppDbContext _dbContext = dbContext;
    private readonly IEnumerable<IBenchmarkValidator> _validators = validators;
    private readonly JudgeDecisionService _judgeDecisionService = judgeDecisionService;

    public async Task<BenchmarkRun> RunAsync(CancellationToken cancellationToken = default)
    {
        var scenarios = await _scenarioLoader.LoadAsync();

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
            foreach (var scenario in scenarios)
            {
                var request = new LLMRequest
                {
                    UserText = scenario.Prompt ?? string.Empty,
                    Capability = scenario.Category,
                    Tone = MapTone(scenario.Tone),
                    Language = MapLanguage(scenario.Language),
                    MaxCharacters = scenario.MaxCharacters,
                    ExpectedSmsSegments = scenario.ExpectedSmsSegments,
                    Creativity = SmsCreativity.Low,
                    UserRequirements = scenario.Requirements
                };

                var responses = await _provider.ExecuteAsync(request, cancellationToken);

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
                        Category = scenario.Category ?? string.Empty,
                        Language = scenario.Language ?? string.Empty,
                        Capability = request.Capability,
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
                    };

                    _dbContext.BenchmarkResults.Add(result);

                    run.TotalExecutions++;
                    if (response.Success)
                    {
                        run.SuccessCount++;
                    }
                    else
                    {
                        run.FailureCount++;
                    }

                    #region Validators
                    var deterministicValidators = _validators.Where(x => x.ValidationType != ValidatorType.LlmJudge);
                    var judgeValidators =_validators.Where(x => x.ValidationType == ValidatorType.LlmJudge);

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
                            var validationResult = await validator.ValidateAsync(scenario, result, cancellationToken);
                            result.Validations.Add(validationResult);
                            _dbContext.BenchmarkValidationResults.Add(validationResult);
                        }
                    }
                    #endregion
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