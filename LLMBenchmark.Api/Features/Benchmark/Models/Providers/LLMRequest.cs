using LLMBenchmark.Api.Features.Benchmark.Enums;

namespace LLMBenchmark.Api.Features.Benchmark.Models.Providers;

public sealed class LLMRequest
{
    public List<string> Models { get; set; } = [];

    public string UserText { get; set; } = default!;

    public SmsAction Action { get; set; }

    public SmsTone Tone { get; set; }

    public SmsLanguage Language { get; set; }

    public SmsCreativity Creativity { get; set; }

    public int? MaxTokens { get; set; }

    public string? Capability { get; set; }

    public int? MaxCharacters { get; set; }
    public int? ExpectedSmsSegments { get; set; }
    public List<string>? UserRequirements { get; set; }
}