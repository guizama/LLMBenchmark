using System.Text.Json.Serialization;

namespace LLMBenchmark.Api.Features.Benchmark.Models.Benchmark;

public sealed class BenchmarkScenario
{
    public string? Id { get; set; }

    public string? Category { get; set; }

    public string? Language { get; set; }

    public string? Tone { get; set; }

    public string? Objective { get; set; }

    public string? Prompt { get; set; }

    [JsonPropertyName("input_text")]
    public string? InputText { get; set; }

    [JsonPropertyName("max_characters")]
    public int? MaxCharacters { get; set; }

    [JsonPropertyName("expected_sms_segments")]
    public int? ExpectedSmsSegments { get; set; }

    public List<string> Requirements { get; set; } = [];

    [JsonPropertyName("expected_behavior")]
    public List<string> ExpectedBehavior { get; set; } = [];
}