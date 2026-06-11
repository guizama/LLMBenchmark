using LLMBenchmark.Api.Features.Benchmark.Enums;
using LLMBenchmark.Api.Features.Benchmark.Models.Benchmark;
using LLMBenchmark.Api.Features.Benchmark.Validators.Contracts;
using LLMBenchmark.Api.Features.Benchmark.Validators.Models;

namespace LLMBenchmark.Api.Features.Benchmark.Validators.Deterministic;

public sealed class SmsSegmentValidator : IBenchmarkValidator
{
    public string Name => "SmsSegmentValidator";
    public ValidatorType ValidationType => ValidatorType.Deterministic;

    public Task<BenchmarkValidationResult> ValidateAsync(BenchmarkScenario scenario, BenchmarkResult result, CancellationToken cancellationToken = default)
    {
        if (!scenario.ExpectedSmsSegments.HasValue)
        {
            return Task.FromResult(
                new BenchmarkValidationResult
                {
                    Id = Guid.NewGuid(),
                    BenchmarkResultId = result.Id,
                    Validator = Name,
                    ValidationType = ValidationType,
                    Passed = true,
                    Details = "Scenario has no expected SMS segments."
                });
        }

        var actualSegments = CalculateSegments(result.Output);
        var expected = scenario.ExpectedSmsSegments.Value;
        var passed = actualSegments <= expected;

        return Task.FromResult(
            new BenchmarkValidationResult
            {
                Id = Guid.NewGuid(),
                BenchmarkResultId = result.Id,
                Validator = Name,
                ValidationType = ValidationType,
                Passed = passed,
                Details = passed
                    ? $"SMS segments OK: {actualSegments}"
                    : $"Expected {expected} segment(s) but got {actualSegments}"
            });
    }

    private static int CalculateSegments(string text)
    {
        var isUnicode = text.Any(c => c > 127);

        var singleLimit = isUnicode ? 70 : 160;
        var multiLimit = isUnicode ? 67 : 153;

        if (text.Length <= singleLimit)
        {
            return 1;
        }

        return (int)Math.Ceiling(text.Length / (double)multiLimit);
    }
}