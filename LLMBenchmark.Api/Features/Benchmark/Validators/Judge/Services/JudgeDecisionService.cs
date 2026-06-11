using LLMBenchmark.Api.Features.Benchmark.Models.Benchmark;
using LLMBenchmark.Api.Features.Benchmark.Validators.Judge.Models;

namespace LLMBenchmark.Api.Features.Benchmark.Validators.Judge.Services;

public sealed class JudgeDecisionService
{
    public JudgeDecision Evaluate(BenchmarkScenario scenario, BenchmarkResult result)
    {
        var reasons = new List<string>();

        if (scenario.ExpectedBehavior.Count > 0)
            reasons.Add("Scenario contains subjective expectations.");

        if (scenario.Category is "rewrite" or "expand" or "summarize")
            reasons.Add("Scenario requires meaning preservation.");

        if (!string.IsNullOrWhiteSpace(scenario.Tone))
            reasons.Add("Scenario requires tone validation.");


        return new JudgeDecision
        {
            ShouldRunJudge = reasons.Count > 0,
            Reasons = reasons
        };
    }
}