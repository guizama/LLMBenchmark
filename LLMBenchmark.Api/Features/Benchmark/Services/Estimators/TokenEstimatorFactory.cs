using LLMBenchmark.Api.Features.Benchmark.Contracts.Estimator;
using LLMBenchmark.Api.Features.Benchmark.Enums;

namespace LLMBenchmark.Api.Features.Benchmark.Services.Estimators;

public sealed class TokenEstimatorFactory(
    SharpTokenEstimator sharpTokenEstimator,
    HeuristicTokenEstimator heuristicTokenEstimator,
    AnthropicApiTokenEstimator anthropicApiTokenEstimator) : ITokenEstimatorFactory
{
    private readonly SharpTokenEstimator _sharpTokenEstimator = sharpTokenEstimator;
    private readonly HeuristicTokenEstimator _heuristicTokenEstimator = heuristicTokenEstimator;
    private readonly AnthropicApiTokenEstimator _anthropicApiTokenEstimator = anthropicApiTokenEstimator;

    public ITokenEstimator Create(TokenizerType tokenizer)
    {
        return tokenizer switch
        {
            // SharpToken supported
            //OpenAi
            TokenizerType.O200KBase => _sharpTokenEstimator,
            TokenizerType.O200KHarmony => _sharpTokenEstimator,
            TokenizerType.Cl100KBase => _sharpTokenEstimator,
            TokenizerType.P50KBase => _sharpTokenEstimator,
            TokenizerType.P50KEdit => _sharpTokenEstimator,
            TokenizerType.R50KBase => _sharpTokenEstimator,
            //Anthropic
            TokenizerType.Claude => _anthropicApiTokenEstimator,

            _ => _heuristicTokenEstimator
        };
    }
}