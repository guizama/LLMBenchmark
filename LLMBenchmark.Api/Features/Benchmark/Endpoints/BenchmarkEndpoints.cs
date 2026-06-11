using LLMBenchmark.Api.Features.Benchmark.Contracts;
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

        app.MapGet("/test-llm", async (ILLMProvider provider) =>
            {
                var response = await provider.ExecuteAsync(
                    new LLMRequest
                    {
                        UserText = "Cria um SMS curto de promoção.",
                        Action = SmsAction.Rewrite,
                        Tone = SmsTone.Neutral,
                        Language = SmsLanguage.PtPT,
                        Creativity = SmsCreativity.Low
                    });

                return response;
            });

        app.MapPost("/benchmark/run", async (BenchmarkRunner runner, CancellationToken cancellationToken) =>
            {
                var result = await runner.RunAsync(cancellationToken);

                return Results.Ok(result);
            });
    }
}