using LLMBenchmark.Api.Features.Benchmark.Models.Benchmark;
using LLMBenchmark.Api.Features.Benchmark.Models.Validation;
using LLMBenchmark.Api.Persistence;

namespace LLMBenchmark.Api.Features.Benchmark.Contracts.Judge;

public interface ILLMJudgeService
{
    Task<ScenarioJudgeResponse> EvaluateAsync(BenchmarkScenario scenario, BenchmarkResult result, CancellationToken cancellationToken = default);
}