using LLMBenchmark.Api.Features.Benchmark.Enums;
using LLMBenchmark.Api.Features.Benchmark.Models.Estimator;

namespace LLMBenchmark.Api.Features.Benchmark.Contracts.Estimator;

public interface ITokenEstimator
{
    Task<TokenEstimateResult> EstimateInputTokensAsync(string model, string systemPrompt, string userPrompt, TokenizerType tokenizer);
}