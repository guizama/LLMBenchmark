using LLMBenchmark.Api.Features.Benchmark.Enums;

namespace LLMBenchmark.Api.Features.Benchmark.Models.Providers;

public sealed class LLMRequest
{
    public string UserText { get; set; } = default!;

    public SmsAction Action { get; set; }

    public SmsTone Tone { get; set; }

    public SmsLanguage Language { get; set; }

    public SmsCreativity Creativity { get; set; }

    public int? MaxTokens { get; set; }
}