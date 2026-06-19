using System.Text.Json.Serialization;

namespace LLMBenchmark.Api.Features.Benchmark.Models.Benchmark;

public sealed class BenchmarkScenario
{
    public string? Id { get; set; }

    public List<string>? Action { get; set; }

    public BenchmarkScenarioInput Input { get; set; } = new();

    public BenchmarkScenarioSource? Source { get; set; }
}

public sealed class BenchmarkScenarioInput
{
    public string? Prompt { get; set; }

    [JsonPropertyName("text")]
    public string? InputText { get; set; }

    public string? Language { get; set; }

    public string? Tone { get; set; }
}

public sealed class BenchmarkScenarioSource
{
    public string? Module { get; set; }

    public Guid? CampaignId { get; set; }

    public Guid? SchedulingId { get; set; }
}