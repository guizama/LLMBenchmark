namespace LLMBenchmark.Api.Persistence;

public sealed class BenchmarkRun
{
    public Guid Id { get; set; }
    public DateTime StartedAtUtc { get; set; }
    public DateTime FinishedAtUtc { get; set; }
    public string Status { get; set; } = "Running";
    public string? Error { get; set; }
    public int TotalScenarios { get; set; }
    public int TotalExecutions { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
}