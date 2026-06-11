using LLMBenchmark.Api.Features.Benchmark.Models.Benchmark;
using LLMBenchmark.Api.Features.Benchmark.Validators.Judge.Models;

namespace LLMBenchmark.Api.Features.Benchmark.Validators.Judge.Contracts;

public interface ILLMJudgeService
{
    Task<ScenarioJudgeResponse> EvaluateAsync(BenchmarkScenario scenario, BenchmarkResult result, CancellationToken cancellationToken = default);
}