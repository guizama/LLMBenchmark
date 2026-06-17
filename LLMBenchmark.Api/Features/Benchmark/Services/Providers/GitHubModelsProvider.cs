using LLMBenchmark.Api.Config;
using LLMBenchmark.Api.Features.Benchmark.Contracts.Estimator;
using LLMBenchmark.Api.Features.Benchmark.Enums;
using LlmTornado;
using LlmTornado.Code;
using LlmTornado.Code.Vendor;
using Microsoft.Extensions.Options;

namespace LLMBenchmark.Api.Features.Benchmark.Services.Providers;

public sealed partial class GitHubModelsProvider(IOptionsMonitor<LLMProviderOptions> options, ITokenEstimatorFactory tokenEstimatorFactory) : BaseLLMProvider(
        options.Get("GitHubModels"),
        tokenEstimatorFactory,
        new TornadoApi(new OpenAiEndpointProvider
            {
                Auth = new ProviderAuthentication(
                    options.Get("GitHubModels").ApiKey),

                UrlResolver = (_, _, _) =>
                    "https://models.github.ai/inference/chat/completions"
            }))
{ 
    public override string ProviderName => "github-models";
    public override BenchmarkProvider BenchmarkProviderType => BenchmarkProvider.GitHubModels;
    public override LLmProviders ProviderType => LLmProviders.Custom;
}