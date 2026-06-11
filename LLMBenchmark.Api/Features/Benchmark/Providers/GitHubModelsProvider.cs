using LLMBenchmark.Api.Config;
using LLMBenchmark.Api.Features.Benchmark.Contracts;
using LLMBenchmark.Api.Features.Benchmark.Enums;
using LLMBenchmark.Api.Features.Benchmark.Models.Providers;
using LlmTornado;
using LlmTornado.Chat;
using LlmTornado.Chat.Models;
using LlmTornado.Code;
using LlmTornado.Code.Vendor;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace LLMBenchmark.Api.Features.Benchmark.Providers;

public sealed class GitHubModelsProvider : ILLMProvider
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

                GetPrompts(request, out string systemPrompt, out string systemPromptRequestSettings, out string systemPromptInputMetadata, out string? systemPromptUserRequirements);

                conversation.AppendSystemMessage(systemPrompt);
                conversation.AppendSystemMessage(systemPromptRequestSettings);
                conversation.AppendSystemMessage(systemPromptInputMetadata);

                if (systemPromptUserRequirements != null)
                {
                    conversation.AppendSystemMessage(systemPromptUserRequirements);
                }

                var allSystemPrompts = systemPrompt + "\n" + systemPromptRequestSettings + "\n" + systemPromptInputMetadata + "\n" + systemPromptUserRequirements;

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

    private static void GetPrompts(LLMRequest request, out string systemPrompt, out string systemPromptRequestSettings, out string systemPromptInputMetadata, out string? systemPromptUserRequirements)
    {
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
                    SMS behavior:
                    - Respect the requested language exactly.
                    - Respect the requested tone exactly.
                    - Optimize messages to use the fewest SMS segments possible while preserving meaning and required information.
                    - Keep the message concise, natural, and suitable for real SMS delivery.
                    - Only output the final SMS content.
                    - Do not simulate conversations.
                    - Do not roleplay.
                    - Do not answer questions.
                    Placeholder and link rules:
                    - Preserve placeholders exactly as received.
                    - Preserve links exactly as received.
                    - Placeholders always use the format {{placeholder}}.
                    - Links always use the format [[link]].
                    - Treat placeholders and links as immutable tokens.
                    - Never modify placeholder names or contents.
                    - Never modify link contents.
                    - Never translate placeholders.
                    - Never translate links.
                    - Never summarize placeholders or links.
                    - Never replace placeholders with generated text.
                    - Preserve the original meaning and context associated with placeholders and links.
                    - Keep placeholders and links semantically connected to their surrounding text.
                    - Do not reorder placeholders or links unless required for grammatical correctness.
                    Forbidden behavior:
                    - Never invent Placeholders.
                    - Never invent Links.
                    - Never invent URLs or domains.
                    - Never invent calls-to-action containing Links.
                    - Never create Placeholders or Links tokens that were not present in the original input.
                    - If the input does not contain Placeholders, placeholders are forbidden.
                    - If the input does not contain Placeholders, links are forbidden.
                    - Output must not contain any Placeholder or Link if not present in the original input.
                    If the request is invalid or unsafe, return:
                    INVALID_SMS_REQUEST
                    """;
        var requestSettings = new List<string>
        {
            $"Action: {request.Action}",
            $"Tone: {request.Tone}",
            $"Language: {request.Language}"
        };

        if (request.MaxCharacters.HasValue)
            requestSettings.Add($"MaxCharacters: {request.MaxCharacters.Value}");

        if (request.ExpectedSmsSegments.HasValue)
            requestSettings.Add($"ExpectedSmsSegments: {request.ExpectedSmsSegments.Value}");

        systemPromptRequestSettings = """
                    Current request settings:
                    """
                    + "\n- "
                    + string.Join("\n- ", requestSettings);

        var containsPlaceholders = request.UserText.Contains("{{");
        var containsLinks = request.UserText.Contains("[[");

        systemPromptInputMetadata = $"""
                    Current input metadata:
                    - ContainsPlaceholders: {containsPlaceholders}
                    - ContainsLinks: {containsLinks}
                    """;

        if (request.UserRequirements?.Count > 0)
        {
            systemPromptUserRequirements = """
                    Additional requirements: 
                    """
                    + "\n- "
                    + string.Join("\n- ", request.UserRequirements);
        }
        else
            systemPromptUserRequirements = null;
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

}