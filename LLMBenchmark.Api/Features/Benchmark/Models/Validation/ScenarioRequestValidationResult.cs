namespace LLMBenchmark.Api.Features.Benchmark.Models.Validation;

public sealed class ScenarioRequestValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = [];
}