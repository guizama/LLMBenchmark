namespace LLMBenchmark.Api.Features.Benchmark.Enums;

public enum TokenizerType
{
    // Generic fallback
    Heuristic = 0,

    // SharpToken / OpenAI encodings
    O200KBase = 1,
    O200KHarmony = 2,
    Cl100KBase = 3,
    P50KBase = 4,
    P50KEdit = 5,
    R50KBase = 6,
    // SharpToken / Anthropic encodings
    Claude = 7,

    // Future tokenizer providers
    AnthropicApi = 100,
    GeminiApi = 101,
    MetaApi = 102,
    HuggingFace = 103
}