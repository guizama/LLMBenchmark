namespace LLMBenchmark.Api.Features.Benchmark.Services.Validation;

public sealed class ScenarioRequestValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = [];
}