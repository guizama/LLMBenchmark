namespace LLMBenchmark.Api.Features.Benchmark.Validators.Judge.Models;

public sealed class ScenarioJudgeResponse
{
    public bool Passed { get; set; }
    public double OverallScore { get; set; }
    public double MeaningPreservation { get; set; }
    public double ToneAdherence { get; set; }
    public double LanguageQuality { get; set; }
    public double InstructionAdherence { get; set; }
    public double SmsSuitability { get; set; }
    public double Safety { get; set; }
    public List<string> Issues { get; set; } = [];
    public string Summary { get; set; } = string.Empty;
    public string? RawResponse { get; set; }
    public int InputTokens { get; set; }
    public int OutputTokens { get; set; }
    public int PredictedInputTokens { get; set; }
    public string? TokenEstimator { get; set; }
    public long EndToEndLatencyMs { get; set; }
    public long? ProviderLatencyMs { get; set; }
    public string? JudgePrompt { get; set; }
    public double InputTokenErrorPercent =>
        InputTokens == 0
            ? 0
            : (InputTokens - PredictedInputTokens) / (double)InputTokens * 100;
}