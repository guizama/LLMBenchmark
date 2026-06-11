using LLMBenchmark.Api.Features.Benchmark.Contracts;
using LLMBenchmark.Api.Features.Benchmark.Enums;

namespace LLMBenchmark.Api.Features.Benchmark.Services.Estimators;

public sealed class TokenEstimatorFactory(
    SharpTokenEstimator sharpTokenEstimator,
    HeuristicTokenEstimator heuristicTokenEstimator) : ITokenEstimatorFactory
{
    private readonly SharpTokenEstimator _sharpTokenEstimator = sharpTokenEstimator;
    private readonly HeuristicTokenEstimator _heuristicTokenEstimator = heuristicTokenEstimator;

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
            TokenizerType.Claude => _sharpTokenEstimator,

            _ => _heuristicTokenEstimator
        };
    }
}