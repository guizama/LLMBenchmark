using LLMBenchmark.Api.Features.Benchmark.Enums;
using LLMBenchmark.Api.Features.Benchmark.Models.Benchmark;
using LLMBenchmark.Api.Persistence;

namespace LLMBenchmark.Api.Features.Benchmark.Contracts.Validator;

public interface IBenchmarkValidator
{
    string Name { get; }
    ValidatorType ValidationType { get; }
    Task<BenchmarkValidationResult> ValidateAsync(BenchmarkScenario scenario, BenchmarkResult result, CancellationToken cancellationToken = default);
}