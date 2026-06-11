using LLMBenchmark.Api.Features.Benchmark.Enums;

namespace LLMBenchmark.Api.Features.Benchmark.Validators.Models;

public sealed class BenchmarkValidationResult
{
    public Guid Id { get; set; }
    public Guid BenchmarkResultId { get; set; }
    public string Validator { get; set; } = default!;
    public ValidatorType ValidationType { get; set; } = default!;
    public bool Passed { get; set; }
    public double? Score { get; set; }
    public string? Details { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    // Judge metadata 
    public string? JudgeProvider { get; set; }
    public string? JudgeModel { get; set; }
    public string? JudgePrompt { get; set; }
    public int? JudgeInputTokens { get; set; }
    public int? JudgeOutputTokens { get; set; }
    public int? JudgePredictedInputTokens { get; set; }
    public long? JudgeLatencyMs { get; set; }
    public decimal? JudgeEstimatedCost { get; set; }
    public string? RawJudgeResponse { get; set; }
    public double? InputTokenErrorPercent { get; set; }
}