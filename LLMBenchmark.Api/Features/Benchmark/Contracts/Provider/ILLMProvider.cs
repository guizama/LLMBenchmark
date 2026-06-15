using LLMBenchmark.Api.Features.Benchmark.Models.Providers;

namespace LLMBenchmark.Api.Features.Benchmark.Contracts.Provider;

public interface ILLMProvider
{
    string ProviderName { get; }

    Task<List<LLMResponse>> ExecuteAsync(LLMRequest request, CancellationToken cancellationToken = default);
}