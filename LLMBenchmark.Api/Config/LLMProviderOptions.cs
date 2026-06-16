using LLMBenchmark.Api.Features.Benchmark.Enums;

namespace LLMBenchmark.Api.Config;

public sealed class LLMProviderOptions
{
    public string ApiKey { get; set; } = default!;

    public List<LLMModelConfig> Models { get; set; } = [];
}

public sealed class LLMModelConfig
{
    public string Vendor { get; set; } = default!;

    public string Model { get; set; } = default!;
    public string Tokenizer { get; set; } = "Heuristic";
}