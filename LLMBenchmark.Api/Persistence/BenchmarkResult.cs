namespace LLMBenchmark.Api.Persistence;

public sealed class BenchmarkResult
{
    public Guid Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string ScenarioId { get; set; } = default!;
    public string Provider { get; set; } = default!;
    public string Model { get; set; } = default!;
    public string Action { get; set; } = default!;
    public string Language { get; set; } = default!;
    public string InputPrompt { get; set; } = default!;
    public string Output { get; set; } = default!;
    public int PredictedInputTokens { get; set; }
    public int InputTokens { get; set; }
    public int OutputTokens { get; set; }
    public int TotalTokens => InputTokens + OutputTokens;
    public decimal EstimatedCost { get; set; }
    public long EndToEndLatencyMs { get; set; }
    public long? ProviderLatencyMs { get; set; }
    public int OutputCharacters { get; set; }
    public int OutputEstimatedSmsSegmentsQtd { get; set; }
    public bool Success { get; set; }
    public string Error { get; set; } = string.Empty;
    public string? RawResponse { get; set; }
    public double Temperature { get; set; }
    public Guid BenchmarkRunId { get; set; }
    public string? TokenEstimator { get; set; }
    public int? InputTokenDelta { get; set; }
    public double? InputTokenErrorPercent { get; set; }

    public List<BenchmarkValidationResult> Validations { get; set; } = [];
    public string? SystemPrompt { get; set; }
}