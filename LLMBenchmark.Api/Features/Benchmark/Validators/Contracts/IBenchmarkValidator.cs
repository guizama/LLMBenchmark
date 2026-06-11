using LLMBenchmark.Api.Features.Benchmark.Enums;
using LLMBenchmark.Api.Features.Benchmark.Models.Benchmark;
using LLMBenchmark.Api.Features.Benchmark.Validators.Models;

namespace LLMBenchmark.Api.Features.Benchmark.Validators.Contracts;

public interface IBenchmarkValidator
{
    string Name { get; }
    ValidatorType ValidationType { get; }
    Task<BenchmarkValidationResult> ValidateAsync(BenchmarkScenario scenario, BenchmarkResult result, CancellationToken cancellationToken = default);
}