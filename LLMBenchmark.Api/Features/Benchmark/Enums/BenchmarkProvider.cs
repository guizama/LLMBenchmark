using System.Text.Json.Serialization;

namespace LLMBenchmark.Api.Features.Benchmark.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum BenchmarkProvider
{
    GitHubModels = 1,
    OpenAI = 2
}