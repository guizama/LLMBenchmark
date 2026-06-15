using System.Text.RegularExpressions;
using LLMBenchmark.Api.Features.Benchmark.Contracts.Validator;
using LLMBenchmark.Api.Features.Benchmark.Enums;
using LLMBenchmark.Api.Features.Benchmark.Models.Benchmark;
using LLMBenchmark.Api.Persistence;

namespace LLMBenchmark.Api.Features.Benchmark.Validators.Deterministic;

public sealed partial class CriticalInfoValidator : IBenchmarkValidator
{
    public string Name => "CriticalInfoValidator";
    public ValidatorType ValidationType => ValidatorType.Deterministic;

    public Task<BenchmarkValidationResult> ValidateAsync(BenchmarkScenario scenario, BenchmarkResult result, CancellationToken cancellationToken = default)
    {
        var inputCriticalData = ExtractCriticalData(result.InputPrompt);
        var outputCriticalData = ExtractCriticalData(result.Output);

        var missing = inputCriticalData.Except(outputCriticalData).ToList();
        var passed = missing.Count == 0;

        return Task.FromResult(
            new BenchmarkValidationResult
            {
                Id = Guid.NewGuid(),
                BenchmarkResultId = result.Id,
                Validator = Name,
                ValidationType = ValidationType,
                Passed = passed,
                Details = passed
                    ? "All critical information preserved."
                    : $"Missing critical data: [{string.Join(", ", missing)}]"
            });
    }

    private static List<string> ExtractCriticalData(string text)
    {
        var matches = new List<string>();

        matches.AddRange(DateRegex().Matches(text).Select(x => x.Value));
        matches.AddRange(MoneyRegex().Matches(text).Select(x => x.Value));
        matches.AddRange(PercentRegex().Matches(text).Select(x => x.Value));
        matches.AddRange(CodeRegex().Matches(text).Select(x => x.Value));

        return [.. matches.Distinct()];
    }

    [GeneratedRegex(@"\b\d{1,2}/\d{1,2}\b")]
    private static partial Regex DateRegex();

    [GeneratedRegex(@"(?:R\$|\$|€)\s?\d+(?:[.,]\d{2})?")]
    private static partial Regex MoneyRegex();

    [GeneratedRegex(@"\b\d+%\b")]
    private static partial Regex PercentRegex();

    [GeneratedRegex(@"\b[A-Z0-9]{4,}\b")]
    private static partial Regex CodeRegex();
}