using System.Text.Json;
using LLMBenchmark.Api.Features.Benchmark.Models.Benchmark;

namespace LLMBenchmark.Api.Features.Benchmark.Services.Scenarios;

public sealed class ScenarioLoader
{
    private static readonly JsonSerializerOptions JsonOptions =
        new()
        {
            PropertyNameCaseInsensitive = true
        };

    public async Task<List<BenchmarkScenario>> LoadAsync()
    {
        var path = Path.Combine(
               AppContext.BaseDirectory,
               "Scenarios",
               "scenarios.json");

        var json = await File.ReadAllTextAsync(path);

        return JsonSerializer.Deserialize<List<BenchmarkScenario>>(json, JsonOptions) ?? [];
    }
}