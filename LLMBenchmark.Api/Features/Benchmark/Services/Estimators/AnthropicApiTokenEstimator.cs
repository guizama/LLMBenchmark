using LLMBenchmark.Api.Config;
using LLMBenchmark.Api.Features.Benchmark.Contracts.Estimator;
using LLMBenchmark.Api.Features.Benchmark.Enums;
using LLMBenchmark.Api.Features.Benchmark.Models.Estimator;
using Microsoft.Extensions.Options;
using SharpToken;
using System.Text;
using System.Text.Json;

namespace LLMBenchmark.Api.Features.Benchmark.Services.Estimators;

public sealed class AnthropicApiTokenEstimator(IHttpClientFactory httpClientFactory, IOptionsMonitor<LLMProviderOptions> options) : ITokenEstimator
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly string _apiKey = options.Get("Anthropic").ApiKey ?? throw new InvalidOperationException("Anthropic ApiKey not configured.");

    public async Task<TokenEstimateResult> EstimateInputTokensAsync(string model, string systemPrompt, string userPrompt, TokenizerType tokenizer)
        {
        using var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Add("x-api-key", _apiKey);
        client.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
        client.Timeout = TimeSpan.FromSeconds(15);

        var payload = new
        {
            model,
            system = systemPrompt,
            messages = new[]
                {
                    new
                    {
                        role = "user",
                        content = new[]
                        {
                            new
                            {
                                type = "text",
                                text = userPrompt
                            }
                        }
                    }
                }
        };

        var json = JsonSerializer.Serialize(payload);

        Console.WriteLine(json);

        var response = await client.PostAsync("https://api.anthropic.com/v1/messages/count_tokens", new StringContent(json, Encoding.UTF8, "application/json"));

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();

            throw new InvalidOperationException($"Anthropic CountTokens failed. " + $"Status={(int)response.StatusCode} " + $"{response.StatusCode}\n" + errorContent);
        }

        var content = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(content);

        var anthropicTokens = doc.RootElement.GetProperty("input_tokens").GetInt32();

        #region AnthropicVsSharpTokenComparison

        var combined = $"{systemPrompt}\n{userPrompt}";

        var sharpClaudeTokens = GptEncoding.GetEncoding("claude").CountTokens(combined);
        var delta = anthropicTokens - sharpClaudeTokens;
        var errorPercent = sharpClaudeTokens == 0 ? 0 : ((double)delta / sharpClaudeTokens) * 100;

        var comparison = new { SharpToken = sharpClaudeTokens, AnthropicApi = anthropicTokens, Delta = delta, ErrorPercent = errorPercent };

        #endregion

        return new TokenEstimateResult
        {
            Model = model,
            EstimatedInputTokens = anthropicTokens,
            Estimator = "anthropic-api",
            Encoding = "anthropic-api"
        };
    }
}