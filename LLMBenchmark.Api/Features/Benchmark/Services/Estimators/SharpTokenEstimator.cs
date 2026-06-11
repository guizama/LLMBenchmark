using LLMBenchmark.Api.Features.Benchmark.Contracts;
using LLMBenchmark.Api.Features.Benchmark.Enums;
using LLMBenchmark.Api.Features.Benchmark.Models.Estimator;
using SharpToken;

namespace LLMBenchmark.Api.Features.Benchmark.Services.Estimators;

public sealed class SharpTokenEstimator : ITokenEstimator
{
    public Task<TokenEstimateResult> EstimateInputTokensAsync(string model, string systemPrompt, string userPrompt, TokenizerType tokenizer)
    {
        var encodingName = GetEncodingName(tokenizer);

        var encoding = GptEncoding.GetEncoding(encodingName);

        var combined = $"{systemPrompt}\n{userPrompt}";

        var tokens = encoding.CountTokens(combined);

        return Task.FromResult(new TokenEstimateResult
        {
            Model = model,
            EstimatedInputTokens = tokens,
            Estimator = "sharp-token",
            Encoding = encodingName
        });
    }

    private static string GetEncodingName(
        TokenizerType tokenizer)
    {
        return tokenizer switch
        {
            TokenizerType.O200KBase => "o200k_base",
            TokenizerType.O200KHarmony => "o200k_harmony",

            TokenizerType.Cl100KBase => "cl100k_base",

            TokenizerType.P50KBase => "p50k_base",
            TokenizerType.P50KEdit => "p50k_edit",

            TokenizerType.R50KBase => "r50k_base",

            TokenizerType.Claude => "claude",

            _ => throw new NotSupportedException(
                $"Tokenizer '{tokenizer}' is not supported by SharpToken.")
        };
    }
}