using LLMBenchmark.Api.Features.Benchmark.Contracts.Provider;
using LLMBenchmark.Api.Features.Benchmark.Enums;
using LLMBenchmark.Api.Features.Benchmark.Models.Providers;
using LLMBenchmark.Api.Features.Benchmark.Services.Runner;
using LLMBenchmark.Api.Features.Benchmark.Services.Scenarios;

namespace LLMBenchmark.Api.Features.Benchmark.Endpoints;

public static class BenchmarkEndpoints
{
    public static void MapBenchmarkEndpoints(this WebApplication app)
    {
        app.MapGet(
            "/benchmark/scenarios",
            async (ScenarioLoader loader) =>
            {
                var scenarios = await loader.LoadAsync();

                return Results.Ok(scenarios);
            });

        app.MapGet("/test-openai", async (IEnumerable<ILLMProvider> providers) =>
        {
            var provider = providers.First(x => x.ProviderName == "openai");

            return await provider.ExecuteAsync(new LLMRequest
            {
                UserText = "Cria um SMS curto de promoção.",
                Action = SmsAction.Generate,
                Tone = SmsTone.Neutral,
                Language = SmsLanguage.PtPT,
                Creativity = SmsCreativity.Low
            });
        });

        app.MapGet("/test-github", async (IEnumerable<ILLMProvider> providers) =>
        {
            var provider = providers.First(x => x.ProviderName == "github-models");

            return await provider.ExecuteAsync(new LLMRequest
            {
                UserText = "Cria um SMS curto de promoção.",
                Action = SmsAction.Generate,
                Tone = SmsTone.Neutral,
                Language = SmsLanguage.PtPT,
                Creativity = SmsCreativity.Low
            });
        });

        app.MapPost("/benchmark/run", async (BenchmarkProvider? provider, ScenariosLoad scenariosLoad, BenchmarkRunner runner, CancellationToken cancellationToken) =>
        {
            var result = await runner.RunAsync(provider, scenariosLoad, cancellationToken);

            return Results.Ok(result);
        });
    }
}