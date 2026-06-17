using LLMBenchmark.Api.Config;
using LLMBenchmark.Api.Features.Benchmark.Contracts.Estimator;
using LLMBenchmark.Api.Features.Benchmark.Enums;
using LlmTornado;
using LlmTornado.Code;
using Microsoft.Extensions.Options;

namespace LLMBenchmark.Api.Features.Benchmark.Services.Providers;

public sealed partial class AnthropicProvider(IOptionsMonitor<LLMProviderOptions> options, ITokenEstimatorFactory tokenEstimatorFactory) : BaseLLMProvider(
        options.Get("Anthropic"),
        tokenEstimatorFactory,
        new TornadoApi(new ProviderAuthentication(LLmProviders.Anthropic, options.Get("Anthropic").ApiKey)))
{
    public override string ProviderName => "anthropic";
    public override BenchmarkProvider BenchmarkProviderType => BenchmarkProvider.Anthropic;
    public override LLmProviders ProviderType => LLmProviders.Anthropic;
}