using System.Diagnostics;
using System.Text;
using System.Text.Json;
using LLMBenchmark.Api.Config;
using LLMBenchmark.Api.Features.Benchmark.Contracts;
using LLMBenchmark.Api.Features.Benchmark.Enums;
using LLMBenchmark.Api.Features.Benchmark.Models.Benchmark;
using LLMBenchmark.Api.Features.Benchmark.Validators.Judge.Contracts;
using LLMBenchmark.Api.Features.Benchmark.Validators.Judge.Models;
using LlmTornado;
using LlmTornado.Chat;
using LlmTornado.Chat.Models;
using LlmTornado.Code;
using LlmTornado.Code.Vendor;
using Microsoft.Extensions.Options;

namespace LLMBenchmark.Api.Features.Benchmark.Validators.Judge.Services;

public sealed class LLMJudgeService : ILLMJudgeService
{
    private readonly TornadoApi _api;
    private readonly ITokenEstimatorFactory _tokenEstimatorFactory;
    private const string JudgeModel = "gpt-4.1-mini";

    public LLMJudgeService(IOptions<GitHubModelsOptions> options, ITokenEstimatorFactory tokenEstimatorFactory)
    {
        _tokenEstimatorFactory = tokenEstimatorFactory;
        _api = new TornadoApi(
            new OpenAiEndpointProvider
            {
                Auth = new ProviderAuthentication(options.Value.ApiKey),
                UrlResolver = (_, _, _) =>
                    "https://models.github.ai/inference/chat/completions"
            });
    }

    public async Task<ScenarioJudgeResponse> EvaluateAsync(BenchmarkScenario scenario, BenchmarkResult result, CancellationToken cancellationToken = default)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(TimeSpan.FromSeconds(60));

        var stopwatch = Stopwatch.StartNew();
        var prompt = BuildPrompt(scenario, result);
        var estimator = _tokenEstimatorFactory.Create(TokenizerType.O200KBase);

        var estimate = await estimator.EstimateInputTokensAsync(JudgeModel, "You are an expert SMS benchmark evaluator.", prompt, TokenizerType.O200KBase);

        var model = new ChatModel(JudgeModel, LLmProviders.Custom);

        var conversation = _api.Chat.CreateConversation(new ChatRequest
                {
                    Model = model,
                    Temperature = 0
                });

        conversation.AppendSystemMessage("""
                You are an expert SMS benchmark evaluator.

                Return ONLY valid JSON.

                Never explain.
                Never use markdown.
                Never add comments.
                Never wrap JSON in code blocks.
                """);

        conversation.AppendUserInput(prompt);

        var response = await conversation.GetResponseRich(cts.Token);

        stopwatch.Stop();

        var content = response.Text ?? string.Empty;

        var parsed =
            JsonSerializer.Deserialize<ScenarioJudgeResponse>(
                content,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? throw new Exception("Failed to parse judge response.");
        long? providerLatency = null;

        if (!string.IsNullOrWhiteSpace(response.RawResponse))
        {
            using var doc = JsonDocument.Parse(response.RawResponse);

            if (doc.RootElement.TryGetProperty("usage", out var usage) &&
                usage.TryGetProperty("latency_checkpoint", out var latencyCheckpoint) &&
                latencyCheckpoint.TryGetProperty("total_duration_ms", out var totalDuration))
            {
                providerLatency = totalDuration.GetInt64();
            }
        }

        parsed.RawResponse = response.RawResponse;
        parsed.InputTokens = response.Usage?.PromptTokens ?? 0;
        parsed.OutputTokens = response.Usage?.CompletionTokens ?? 0;
        parsed.PredictedInputTokens = estimate.EstimatedInputTokens;
        parsed.TokenEstimator = estimate.Estimator;
        parsed.EndToEndLatencyMs = stopwatch.ElapsedMilliseconds;
        parsed.ProviderLatencyMs = providerLatency;
        parsed.JudgePrompt = prompt;

        return parsed;
    }

    private static string BuildPrompt(BenchmarkScenario scenario, BenchmarkResult result)
    {
        var requirements = scenario.Requirements.Count > 0
                ? string.Join("\n- ", scenario.Requirements)
                : "None";

        var expectedBehavior = scenario.ExpectedBehavior.Count > 0
                ? string.Join("\n- ", scenario.ExpectedBehavior)
                : "None";

        var sb = new StringBuilder();

        sb.AppendLine("""
                Return ONLY valid JSON.

                Expected JSON format:
                {
                    "passed": true,
                    "overallScore": 8.5,
                    "meaningPreservation": 9,
                    "toneAdherence": 8,
                    "languageQuality": 10,
                    "instructionAdherence": 8,
                    "smsSuitability": 9,
                    "safety": 10,
                    "issues": [],
                    "summary": "Good SMS generation."
                }
                """);

        sb.AppendLine($"Category: {scenario.Category}");
        sb.AppendLine($"Language: {scenario.Language}");
        sb.AppendLine($"Tone: {scenario.Tone}");

        sb.AppendLine("\nRequirements:");
        sb.AppendLine($"- {requirements}");

        sb.AppendLine("\nExpected Behavior:");
        sb.AppendLine($"- {expectedBehavior}");

        sb.AppendLine("\nSystem Prompt:");
        sb.AppendLine(result.SystemPrompt);

        sb.AppendLine("\nInput:");
        sb.AppendLine(result.InputPrompt);

        sb.AppendLine("\nOutput:");
        sb.AppendLine(result.Output);

        return sb.ToString();
    }
}