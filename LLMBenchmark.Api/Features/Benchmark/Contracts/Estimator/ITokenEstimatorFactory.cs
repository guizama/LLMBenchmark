using LLMBenchmark.Api.Features.Benchmark.Enums;

namespace LLMBenchmark.Api.Features.Benchmark.Contracts.Estimator;

public interface ITokenEstimatorFactory
{
    ITokenEstimator Create(TokenizerType tokenizer);
}