using System.Text.Json.Serialization;

namespace LLMBenchmark.Api.Features.Benchmark.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ScenariosLoad
{
    Simple = 1,
    Full = 2
}