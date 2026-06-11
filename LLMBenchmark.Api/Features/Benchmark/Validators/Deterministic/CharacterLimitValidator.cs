using LLMBenchmark.Api.Features.Benchmark.Enums;
using LLMBenchmark.Api.Features.Benchmark.Models.Benchmark;
using LLMBenchmark.Api.Features.Benchmark.Validators.Contracts;
using LLMBenchmark.Api.Features.Benchmark.Validators.Models;

namespace LLMBenchmark.Api.Features.Benchmark.Validators.Deterministic;

public sealed class CharacterLimitValidator : IBenchmarkValidator
{
    public string Name => "CharacterLimitValidator";
    public ValidatorType ValidationType => ValidatorType.Deterministic;

    public Task<BenchmarkValidationResult> ValidateAsync(BenchmarkScenario scenario, BenchmarkResult result, CancellationToken cancellationToken = default)
    {
        if (!scenario.MaxCharacters.HasValue)
        {
            return Task.FromResult(
                new BenchmarkValidationResult
                {
                    Id = Guid.NewGuid(),
                    BenchmarkResultId = result.Id,
                    Validator = Name,
                    ValidationType = ValidationType,
                    Passed = true,
                    Details = "Scenario has no max character limit."
                });
        }

        var currentLength = result.Output.Length;
        var limit = scenario.MaxCharacters.Value;

        var passed = currentLength <= limit;

        return Task.FromResult(
            new BenchmarkValidationResult
            {
                Id = Guid.NewGuid(),
                BenchmarkResultId = result.Id,
                Validator = Name,
                ValidationType = ValidationType,
                Passed = passed,
                Details = passed
                    ? $"Length OK: {currentLength}/{limit}"
                    : $"Length exceeded: {currentLength}/{limit}"
            });
    }
}