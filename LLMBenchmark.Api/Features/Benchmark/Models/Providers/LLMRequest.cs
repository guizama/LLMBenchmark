using LLMBenchmark.Api.Features.Benchmark.Enums;

namespace LLMBenchmark.Api.Features.Benchmark.Models.Providers;

public sealed class LLMRequest
{
    public string UserText { get; set; } = default!;

    public List<SmsAction> Action { get; set; } = new();

    public SmsTone Tone { get; set; }

    public SmsLanguage Language { get; set; }

    public SmsCreativity Creativity { get; set; }

    public int? MaxTokens { get; set; }
}