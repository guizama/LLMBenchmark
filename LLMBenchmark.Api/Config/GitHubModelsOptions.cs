using LLMBenchmark.Api.Features.Benchmark.Enums;

namespace LLMBenchmark.Api.Config;

public sealed class GitHubModelsOptions
{
    public string ApiKey { get; set; } = default!;

    public List<GitHubModelConfig> Models { get; set; } = [];
}

public sealed class GitHubModelConfig
{
    public string Vendor { get; set; } = default!;

    public string Model { get; set; } = default!;
    public string Tokenizer { get; set; } = "Heuristic";
}