namespace LLMBenchmark.Api.Features.Benchmark.Models.Estimator;

public sealed class TokenEstimateResult
{
    public string Model { get; set; } = default!;

    public int EstimatedInputTokens { get; set; }

    public string Estimator { get; set; } = default!;
    public string Encoding { get; set; } = default!;
}
