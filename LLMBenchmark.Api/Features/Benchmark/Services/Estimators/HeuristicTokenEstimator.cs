using LLMBenchmark.Api.Features.Benchmark.Contracts.Estimator;
using LLMBenchmark.Api.Features.Benchmark.Enums;
using LLMBenchmark.Api.Features.Benchmark.Models.Estimator;

namespace LLMBenchmark.Api.Features.Benchmark.Services.Estimators;

public sealed class HeuristicTokenEstimator : ITokenEstimator
{
    public Task<TokenEstimateResult> EstimateInputTokensAsync(string model, string systemPrompt, string userPrompt, TokenizerType tokenizer)
    {
        var combined = systemPrompt + userPrompt;

        // conta simples de token
        // ~4 caracteres = 1 token
        var estimatedTokens = combined.Length / 4;

        return Task.FromResult(new TokenEstimateResult
        {
            Model = model,
            EstimatedInputTokens = estimatedTokens,
            Estimator = "Heuristic",
            Encoding = "heuristic"
        });
    }
}