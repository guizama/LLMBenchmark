using LLMBenchmark.Api.Features.Benchmark.Enums;
using LLMBenchmark.Api.Features.Benchmark.Models.Providers;
using LlmTornado.Code;

namespace LLMBenchmark.Api.Features.Benchmark.Contracts.Provider;

public interface ILLMProvider
{
    BenchmarkProvider BenchmarkProviderType { get; }
    string ProviderName { get; }
    LLmProviders ProviderType { get; }

    Task<List<LLMResponse>> ExecuteAsync(LLMRequest request, CancellationToken cancellationToken = default);
}