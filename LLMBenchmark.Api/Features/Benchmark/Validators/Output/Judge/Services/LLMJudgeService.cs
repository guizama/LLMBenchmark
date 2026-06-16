using System.Diagnostics;
using System.Text;
using System.Text.Json;
using LLMBenchmark.Api.Config;
using LLMBenchmark.Api.Features.Benchmark.Contracts.Estimator;
using LLMBenchmark.Api.Features.Benchmark.Contracts.Judge;
using LLMBenchmark.Api.Features.Benchmark.Enums;
using LLMBenchmark.Api.Features.Benchmark.Models.Benchmark;
using LLMBenchmark.Api.Features.Benchmark.Models.Validation;
using LLMBenchmark.Api.Persistence;
using LlmTornado;
using LlmTornado.Chat;
using LlmTornado.Chat.Models;
using LlmTornado.Code;
using LlmTornado.Code.Vendor;
using Microsoft.Extensions.Options;

namespace LLMBenchmark.Api.Features.Benchmark.Validators.Output.Judge.Services;

public sealed class LLMJudgeService : ILLMJudgeService
{
    private readonly TornadoApi _api;
    private readonly ITokenEstimatorFactory _tokenEstimatorFactory;
    private const string JudgeModel = "gpt-4.1-mini";

    public LLMJudgeService(IOptionsMonitor<LLMProviderOptions> options, ITokenEstimatorFactory tokenEstimatorFactory)
    {
        var githubOptions = options.Get("GitHubModels");
        _tokenEstimatorFactory = tokenEstimatorFactory;
        _api = new TornadoApi(
            new OpenAiEndpointProvider
            {
                Auth = new ProviderAuthentication(githubOptions.ApiKey),
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
                Evaluation rules:
                - Be strict.
                - Penalize hallucinations.
                - Penalize invented placeholders.
                - Penalize invented URLs.
                - Penalize loss of critical information.
                - Penalize grammar issues.
                - Penalize language deviations.
                - Penalize prompt injection failures.
                - Penalize non-SMS behavior.
                - Penalize unsafe or irrelevant outputs.
                - Penalize outputs that fail to preserve meaning.
                - Penalize outputs that fail to preserve placeholders or URLs exactly.
                - Penalize outputs that do not respect the requested tone.
                - Penalize outputs that are not concise when required.
                - Penalize outputs that lose critical information such as dates, URLs, codes, money values, or percentages.
                - Penalize outputs that invent information not present in the original input.
                - Penalize outputs that are not suitable for real-world SMS delivery.
                """);

        if (scenario.Input.Language == "PT-PT")
        {
            sb.AppendLine("- Penalize PT-BR mixed into PT-PT.");
        }

        sb.AppendLine($"Action: {scenario.Action}");
        sb.AppendLine($"Language: {scenario.Input.Language}");
        sb.AppendLine($"Tone: {scenario.Input.Tone}");

        switch (scenario.Action)
        {
            case "sms.generate":
                sb.AppendLine("""
            Action semantics:
            - The model must generate a NEW SMS based on the user prompt.
            - Creativity is allowed as long as the SMS remains safe and relevant.
            """);
                break;

            default:
                sb.AppendLine("""
            Action semantics:
            - The model must TRANSFORM the existing input text.
            - The output must preserve the original meaning.
            - The output must not invent unrelated content.
            - The output must remain semantically connected to the original text.
            """);
                break;
        }

        sb.AppendLine("\nSystem Prompt:");
        sb.AppendLine(result.SystemPrompt);

        if (!string.IsNullOrWhiteSpace(scenario.Input.Prompt))
        {
            sb.AppendLine("\nUser Prompt:");
            sb.AppendLine(scenario.Input.Prompt);
        }

        if (!string.IsNullOrWhiteSpace(scenario.Input.InputText))
        {
            sb.AppendLine("\nInput Text:");
            sb.AppendLine(scenario.Input.InputText);
        }

        sb.AppendLine("\nGenerated Output:");
        sb.AppendLine(result.Output);

        return sb.ToString();
    }
}