using LLMBenchmark.Api.Features.Benchmark.Enums;
using LLMBenchmark.Api.Features.Benchmark.Helpers;
using LLMBenchmark.Api.Features.Benchmark.Models.Benchmark;
using LLMBenchmark.Api.Features.Benchmark.Models.Validation;
using LLMBenchmark.Api.Persistence;

namespace LLMBenchmark.Api.Features.Benchmark.Validators.Output.Judge.Services;

public sealed class JudgeDecisionService
{
    public JudgeDecision Evaluate(BenchmarkScenario scenario, BenchmarkResult result)
    {
        var reasons = new List<string>();

        var actionList = scenario.Action ?? new List<string>();

        foreach (var actionStr in actionList)
        {
            if (SmsActionParser.TryParse(actionStr, out SmsAction action))
            {
                switch (action)
                {
                    case SmsAction.Generate:
                        reasons.Add("Generate action requires semantic and quality evaluation.");
                        break;
                    case SmsAction.Rewrite:
                        reasons.Add("Rewrite action requires meaning preservation validation.");
                        break;
                    case SmsAction.Shorten:
                        reasons.Add("Shorten action requires compression quality validation.");
                        break;
                    case SmsAction.Expand:
                        reasons.Add("Expand action requires expansion quality validation.");
                        break;
                    case SmsAction.Formalize:
                        reasons.Add("Formalize action requires tone transformation validation.");
                        break;
                    case SmsAction.Casualize:
                        reasons.Add("Casualize action requires tone transformation validation.");
                        break;
                    case SmsAction.FixGrammar:
                        reasons.Add("FixGrammar action requires grammar correction validation.");
                        break;
                }
            }
        }

        if (!string.IsNullOrWhiteSpace(scenario.Input.Tone))
            reasons.Add("Scenario requires tone adherence validation.");

        if (!string.IsNullOrWhiteSpace(result.Output))
            reasons.Add("Scenario requires SMS quality evaluation.");


        return new JudgeDecision
        {
            ShouldRunJudge = reasons.Count > 0,
            Reasons = [.. reasons.Distinct()]
        };
    }
}