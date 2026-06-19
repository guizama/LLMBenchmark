using LLMBenchmark.Api.Features.Benchmark.Helpers;
using LLMBenchmark.Api.Features.Benchmark.Models.Benchmark;
using LLMBenchmark.Api.Features.Benchmark.Models.Validation;

namespace LLMBenchmark.Api.Features.Benchmark.Validators.Input;

public sealed class ScenarioRequestValidator
{
    private static readonly HashSet<string> Languages =
    [
        "pt-PT",
        "en-US"
    ];

    private static readonly HashSet<string> Tones =
    [
        "neutral",
        "casual",
        "formal"
    ];

    public ScenarioRequestValidationResult Validate(BenchmarkScenario scenario)
    {
        var errors = new List<string>();

        if (scenario.Action == null || scenario.Action.Count == 0)
            errors.Add("Action is required.");
        else
        {
            foreach (var action in scenario.Action)
            {
                if (string.IsNullOrWhiteSpace(action))
                    errors.Add("Action cannot be empty.");
                else if (!SmsActionParser.TryParse(action, out _))
                    errors.Add($"Invalid action: {action}");
            }
        }

        if (scenario.Input.Language is null || !Languages.Contains(scenario.Input.Language))
            errors.Add($"Invalid language: {scenario.Input.Language}");

        if (scenario.Input.Tone is null || !Tones.Contains(scenario.Input.Tone))
            errors.Add($"Invalid tone: {scenario.Input.Tone}");

        var requiresPrompt = scenario.Action != null && scenario.Action.Contains("sms.generate");
        var requiresText = scenario.Action != null && (
            scenario.Action.Contains("sms.rewrite") ||
            scenario.Action.Contains("sms.shorten") ||
            scenario.Action.Contains("sms.expand") ||
            scenario.Action.Contains("sms.formalize") ||
            scenario.Action.Contains("sms.casualize") ||
            scenario.Action.Contains("sms.fixGrammar"));

        if (requiresPrompt && string.IsNullOrWhiteSpace(scenario.Input.Prompt))
            errors.Add("input.prompt is required.");

        if (requiresText && string.IsNullOrWhiteSpace(scenario.Input.InputText))
            errors.Add("input.text is required.");

        return new ScenarioRequestValidationResult
        {
            IsValid = errors.Count == 0,
            Errors = errors
        };
    }
}