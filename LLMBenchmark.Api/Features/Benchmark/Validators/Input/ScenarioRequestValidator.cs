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

        if (string.IsNullOrWhiteSpace(scenario.Action))
            errors.Add("Action is required.");
        else if (!SmsActionParser.TryParse(scenario.Action, out _))
            errors.Add($"Invalid action: {scenario.Action}");

        if (scenario.Input.Language is null || !Languages.Contains(scenario.Input.Language))
            errors.Add($"Invalid language: {scenario.Input.Language}");

        if (scenario.Input.Tone is null || !Tones.Contains(scenario.Input.Tone))
            errors.Add($"Invalid tone: {scenario.Input.Tone}");

        var requiresPrompt = scenario.Action == "sms.generate";
        var requiresText = scenario.Action is "sms.rewrite" or "sms.shorten" or "sms.expand" or "sms.formalize" or "sms.casualize" or "sms.fixGrammar";

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