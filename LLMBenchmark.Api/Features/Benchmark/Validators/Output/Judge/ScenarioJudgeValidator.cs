using System.Text.Json;
using LLMBenchmark.Api.Features.Benchmark.Contracts.Judge;
using LLMBenchmark.Api.Features.Benchmark.Contracts.Validator;
using LLMBenchmark.Api.Features.Benchmark.Enums;
using LLMBenchmark.Api.Features.Benchmark.Models.Benchmark;
using LLMBenchmark.Api.Persistence;

namespace LLMBenchmark.Api.Features.Benchmark.Validators.Output.Judge;

public sealed class ScenarioJudgeValidator(ILLMJudgeService judgeService) : IBenchmarkValidator
{
    private readonly ILLMJudgeService _judgeService = judgeService;
    public string Name => "ScenarioJudgeValidator";

    public ValidatorType ValidationType => ValidatorType.LlmJudge;

    public async Task<BenchmarkValidationResult> ValidateAsync(BenchmarkScenario scenario, BenchmarkResult result, CancellationToken cancellationToken = default)
    {
        var judge = _judgeService.EvaluateAsync(scenario, result, cancellationToken);

        return new BenchmarkValidationResult
        {
            Id = Guid.NewGuid(),
            BenchmarkResultId = result.Id,
            Validator = Name,
            ValidationType = ValidationType,

            Passed = judge.Result.Passed,
            Score = judge.Result.OverallScore,
            Details = judge.Result.Summary,

            JudgeProvider = "GitHubModels",
            JudgeModel = "gpt-4.1-mini",
            JudgePrompt = judge.Result.JudgePrompt,

            JudgeInputTokens = judge.Result.InputTokens,
            JudgeOutputTokens = judge.Result.OutputTokens,
            JudgePredictedInputTokens = judge.Result.PredictedInputTokens,
            InputTokenErrorPercent = judge.Result.InputTokenErrorPercent,

            JudgeLatencyMs = judge.Result.EndToEndLatencyMs,
            JudgeEstimatedCost = 0,

            RawJudgeResponse = JsonSerializer.Serialize(judge)
        };
    }
}