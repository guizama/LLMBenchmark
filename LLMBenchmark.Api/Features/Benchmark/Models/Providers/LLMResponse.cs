public sealed class LLMResponse
{
    public string Provider { get; set; } = default!;
    public string Model { get; set; } = default!;
    public string Output { get; set; } = default!;
    public double Temperature { get; set; }
    public bool Success { get; set; }
    public string Error { get; set; } = string.Empty;
    public string? RawResponse { get; set; }
    public string? SystemPrompt { get; set; }
    public LLMResponseTokens Tokens { get; set; } = new();
    public LLMResponseLatency Latency { get; set; } = new();
    public LLMResponseInputPrediction? InputPrediction { get; set; }
}

public sealed class LLMResponseTokens
{
    public int InputTokens { get; set; }
    public int OutputTokens { get; set; }
    public int TotalTokens { get; set; }
}

public sealed class LLMResponseLatency
{
    public long EndToEndLatencyMs { get; set; }
    public long? ProviderLatencyMs { get; set; }
}

public sealed class LLMResponseInputPrediction
{
    public int PredictedInputTokens { get; set; }
    public string? TokenEstimator { get; set; }
    public int ActualInputTokens { get; set; }

    public int InputTokenDelta => ActualInputTokens - PredictedInputTokens;
    public double InputTokenErrorPercent =>
        ActualInputTokens == 0
            ? 0
            : (ActualInputTokens - PredictedInputTokens) / (double)ActualInputTokens * 100;
}