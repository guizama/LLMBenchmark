namespace LLMBenchmark.Api.Features.Benchmark.Validators.Judge.Models;

public sealed class JudgeDecision
{
    public bool ShouldRunJudge { get; set; }
    public List<string> Reasons { get; set; } = [];
}