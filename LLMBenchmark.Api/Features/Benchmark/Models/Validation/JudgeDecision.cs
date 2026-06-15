namespace LLMBenchmark.Api.Features.Benchmark.Models.Validation;

public sealed class JudgeDecision
{
    public bool ShouldRunJudge { get; set; }
    public List<string> Reasons { get; set; } = [];
}