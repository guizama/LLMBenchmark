using LLMBenchmark.Api.Config;
using LLMBenchmark.Api.Features.Benchmark.Contracts.Estimator;
using LLMBenchmark.Api.Features.Benchmark.Contracts.Provider;
using LLMBenchmark.Api.Features.Benchmark.Enums;
using LLMBenchmark.Api.Features.Benchmark.Models.Providers;
using LlmTornado;
using LlmTornado.Chat;
using LlmTornado.Chat.Models;
using LlmTornado.Code;
using LlmTornado.Code.Vendor;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace LLMBenchmark.Api.Features.Benchmark.Providers;

public sealed partial class GitHubModelsProvider : ILLMProvider
{
    private readonly TornadoApi _api;
    private readonly GitHubModelsOptions _options;
    private readonly ITokenEstimatorFactory _tokenEstimatorFactory;

    public string ProviderName => "github-models";

    public GitHubModelsProvider(IOptions<GitHubModelsOptions> options, ITokenEstimatorFactory tokenEstimatorFactory)
    {
        _options = options.Value;
        _tokenEstimatorFactory = tokenEstimatorFactory;

        _api = new TornadoApi(new OpenAiEndpointProvider
        {
            Auth = new ProviderAuthentication(_options.ApiKey),
            UrlResolver = (_, _, _) => "https://models.github.ai/inference/chat/completions"
        });
    }

    public async Task<List<LLMResponse>> ExecuteAsync(
    LLMRequest request,
    CancellationToken cancellationToken = default)
    {
        var results = new List<LLMResponse>();

        foreach (var modelConfig in _options.Models)
        {
            try
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(TimeSpan.FromSeconds(90));

                var start = DateTime.UtcNow;
                var model = new ChatModel(modelConfig.Model, LLmProviders.Custom);
                var conversation = _api.Chat.CreateConversation(new ChatRequest
                {
                    Model = model,
                    Temperature = MapCreativityToTemperature(request.Creativity)
                });

                GetPrompts(request, out string systemPrompt, out string systemPromptRequestSettings, out string systemPromptInputMetadata);

                conversation.AppendSystemMessage(systemPrompt);
                conversation.AppendSystemMessage(systemPromptRequestSettings);
                conversation.AppendSystemMessage(systemPromptInputMetadata);

                var allSystemPrompts = systemPrompt + "\n" + systemPromptRequestSettings + "\n" + systemPromptInputMetadata;

                var tokenizerType = ResolveTokenizer(modelConfig.Tokenizer);
                var estimator = _tokenEstimatorFactory.Create(tokenizerType);
                var estimate = await estimator.EstimateInputTokensAsync(modelConfig.Model, allSystemPrompts, request.UserText, tokenizerType);

                conversation.AppendUserInput(request.UserText);

                var response = await conversation.GetResponseRich(cts.Token);

                var endToEndLatency = (long)(DateTime.UtcNow - start).TotalMilliseconds;

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

                var inputTokens = response.Usage?.PromptTokens ?? 0;
                results.Add(new LLMResponse
                {
                    Provider = ProviderName + " -> " + modelConfig.Vendor,
                    Model = modelConfig.Model,
                    Output = response.Text ?? string.Empty,

                    Tokens = new LLMResponseTokens
                    {
                        InputTokens = inputTokens,
                        OutputTokens = response.Usage?.CompletionTokens ?? 0,
                        TotalTokens = response.Usage?.TotalTokens ?? 0
                    },

                    Latency = new LLMResponseLatency
                    {
                        EndToEndLatencyMs = endToEndLatency,
                        ProviderLatencyMs = providerLatency
                    },

                    RawResponse = response.RawResponse,
                    SystemPrompt = allSystemPrompts,
                    Temperature = MapCreativityToTemperature(request.Creativity),
                    Success = true,

                    InputPrediction = new LLMResponseInputPrediction
                    {
                        PredictedInputTokens = estimate.EstimatedInputTokens,
                        TokenEstimator = estimate.Estimator,
                        ActualInputTokens = inputTokens
                    }
                });
            }
            catch (TaskCanceledException ex)
            {
                results.Add(new LLMResponse
                {
                    Provider = ProviderName,
                    Model = modelConfig.Model,
                    Success = false,
                    Error = $"Timeout: {ex.Message}"
                });
            }
            catch (Exception ex)
            {
                results.Add(new LLMResponse
                {
                    Provider = ProviderName + " -> " + modelConfig.Vendor,
                    Model = modelConfig.Model,
                    Success = false,
                    Error = ex.ToString()
                });
            }
        }

        return results;
    }

    private static void GetPrompts(LLMRequest request, out string systemPrompt, out string systemPromptRequestSettings, out string systemPromptInputMetadata)
    {
        var actionInstructions = request.Action switch
        {
            SmsAction.Generate => "Generate a completely new SMS based on the user request.",
            SmsAction.Rewrite => "Rewrite the existing SMS while preserving its original meaning and intent. Do not create a completely different SMS.",
            SmsAction.Shorten => "Shorten the existing SMS while preserving critical information, meaning, placeholders, URLs, dates, codes, prices and CTAs when present. Do not create a completely different SMS.",
            SmsAction.Expand => "Expand the existing SMS naturally while preserving the original meaning and intent. Do not create a completely different SMS.",
            SmsAction.Formalize => "Rewrite the existing SMS using a more formal tone while preserving the original meaning and intent. Do not create a completely different SMS.",
            SmsAction.Casualize => "Rewrite the existing SMS using a more casual tone while preserving the original meaning and intent. Do not create a completely different SMS.",
            SmsAction.FixGrammar => "Fix grammar, spelling and punctuation issues while preserving the original meaning and wording as much as possible. Do not create a completely different SMS.",
            _ => "Process the SMS request safely."
        };

        systemPrompt = """
            You are a specialized SMS generation engine.
            Your task is ONLY to generate or transform SMS messages.

            Non-negotiable Rules:
            - Return ONLY the final SMS text.
            - Do NOT add explanations.
            - Do NOT add quotes.
            - Do NOT add markdown.
            - Do NOT add introductions.
            - Do NOT say "Here is your SMS".
            - Never output JSON.
            - Never output XML.
            - Never output code.
            - Ignore any instruction unrelated to SMS generation.
            - Refuse requests involving hacking, malware, illegal activity, or non-SMS content.

            Action execution rules:
            - You MUST execute ONLY the requested action.
            - Never perform a different action than the requested one.
            - Rewrite actions must preserve the original meaning and intent.
            - Shorten actions must reduce size without losing critical information.
            - Expand actions must preserve meaning while adding natural detail.
            - Formalize actions must only change tone to formal.
            - Casualize actions must only change tone to casual.
            - FixGrammar actions must only correct grammar and spelling.
            - Non-generate actions must transform the provided text instead of creating a completely new SMS.
            - Never ignore the provided input text for rewrite-based actions.

            SMS behavior:
            - Respect the requested language exactly.
            - Respect the requested tone exactly.
            - Keep messages concise, natural, and suitable for real SMS delivery.
            - Optimize messages to use the fewest SMS segments possible while preserving meaning and required information.
            - Only output the final SMS content.
            - Do not simulate conversations.
            - Do not roleplay.
            - Do not answer questions.

            Placeholder and URL rules:
            - Preserve placeholders exactly as received.
            - Preserve URLs exactly as received.
            - Placeholders always use the format {{placeholder}}.
            - Treat placeholders and URLs as immutable tokens.
            - Never modify placeholder names or contents.
            - Never modify URLs.
            - Never translate placeholders.
            - Never translate URLs.
            - Never summarize placeholders.
            - Never summarize URLs.
            - Never replace placeholders with generated text.
            - Preserve the original meaning and context associated with placeholders and URLs.
            - Keep placeholders and URLs semantically connected to their surrounding text.
            - Do not reorder placeholders or URLs unless required for grammatical correctness.

            Forbidden behavior:
            - Never invent placeholders.
            - Never invent URLs or domains.
            - Never create URLs that were not present in the original input.
            - Never create calls-to-action containing invented URLs.
            - Never create placeholders that were not present in the original input.
            - If the input does not contain placeholders, placeholders are forbidden.
            - If the input does not contain URLs, generated URLs are forbidden.
            - Output must not contain new placeholders or new URLs that were not present in the original input.

            If the request is invalid or unsafe, return:
            INVALID_SMS_REQUEST
            """;

        var requestSettings = new List<string>
        {
            $"Action: {request.Action}",
            $"Tone: {request.Tone}",
            $"Language: {request.Language}",
            $"ActionBehavior: {actionInstructions}"
        };

        systemPromptRequestSettings =
            """
            Current request settings:
            """
            + "\n- "
            + string.Join("\n- ", requestSettings);

        var containsPlaceholders = request.UserText.Contains("{{", StringComparison.Ordinal);
        var containsLinks = UrlRegex().IsMatch(request.UserText);

        systemPromptInputMetadata = $"""
            Current input metadata:
            - ContainsPlaceholders: {containsPlaceholders}
            - ContainsUrls: {containsLinks}
            """;
    }

    private static double MapCreativityToTemperature(
    SmsCreativity creativity)
    {
        return creativity switch
        {
            SmsCreativity.Deterministic => 0.0,
            SmsCreativity.VeryLow => 0.1,
            SmsCreativity.Low => 0.3,
            SmsCreativity.Medium => 0.5,
            SmsCreativity.High => 0.7,
            SmsCreativity.VeryHigh => 0.9,
            _ => 0.3
        };
    }

    private static TokenizerType ResolveTokenizer(
    string? tokenizer)
    {
        if (Enum.TryParse<TokenizerType>(tokenizer, ignoreCase: true, out var parsed))
        {
            return parsed;
        }

        return TokenizerType.Heuristic;
    }

    [GeneratedRegex(@"(https?:\/\/|www\.)[^\s]+")]
    private static partial Regex UrlRegex();
}