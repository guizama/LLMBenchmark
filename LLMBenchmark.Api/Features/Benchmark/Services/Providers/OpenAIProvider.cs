using LLMBenchmark.Api.Config;
using LLMBenchmark.Api.Features.Benchmark.Contracts.Estimator;
using LLMBenchmark.Api.Features.Benchmark.Enums;
using LlmTornado;
using LlmTornado.Code;
using Microsoft.Extensions.Options;

namespace LLMBenchmark.Api.Features.Benchmark.Services.Providers;

public sealed partial class OpenAIProvider(IOptionsMonitor<LLMProviderOptions> options, ITokenEstimatorFactory tokenEstimatorFactory) : BaseLLMProvider(
        options.Get("OpenAI"),
        tokenEstimatorFactory,
        new TornadoApi(new ProviderAuthentication(LLmProviders.OpenAi, options.Get("OpenAI").ApiKey)))
{
    public override string ProviderName => "openai";
    public override BenchmarkProvider BenchmarkProviderType => BenchmarkProvider.OpenAI;
    public override LLmProviders ProviderType => LLmProviders.OpenAi;
}