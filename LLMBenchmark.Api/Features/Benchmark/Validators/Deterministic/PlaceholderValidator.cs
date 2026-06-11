using System.Text.RegularExpressions;
using LLMBenchmark.Api.Features.Benchmark.Enums;
using LLMBenchmark.Api.Features.Benchmark.Models.Benchmark;
using LLMBenchmark.Api.Features.Benchmark.Validators.Contracts;
using LLMBenchmark.Api.Features.Benchmark.Validators.Models;

namespace LLMBenchmark.Api.Features.Benchmark.Validators.Deterministic;

public sealed partial class PlaceholderValidator : IBenchmarkValidator
{
    public string Name => "PlaceholderValidator";
    public ValidatorType ValidationType => ValidatorType.Deterministic;

    public Task<BenchmarkValidationResult> ValidateAsync(BenchmarkScenario scenario, BenchmarkResult result, CancellationToken cancellationToken = default)
    {
        var inputPlaceholders = ExtractPlaceholders(result.InputPrompt);
        var outputPlaceholders = ExtractPlaceholders(result.Output);

        var missing = inputPlaceholders.Except(outputPlaceholders).ToList();
        var extra = outputPlaceholders.Except(inputPlaceholders).ToList();

        var orderOk = inputPlaceholders.SequenceEqual(outputPlaceholders);

        var inputCounts = inputPlaceholders.GroupBy(x => x).ToDictionary(x => x.Key, x => x.Count());
        var outputCounts = outputPlaceholders.GroupBy(x => x).ToDictionary(x => x.Key, x => x.Count());

        var countOk =
            inputCounts.Count == outputCounts.Count &&
            inputCounts.All(x =>
                outputCounts.TryGetValue(x.Key, out var count) &&
                count == x.Value);

        var passed = 
            missing.Count == 0 && extra.Count == 0 && orderOk && countOk;

        var details = new List<string>();

        if (missing.Count > 0)
            details.Add($"Missing: [{string.Join(", ", missing)}]");

        if (extra.Count > 0)
            details.Add($"Extra: [{string.Join(", ", extra)}]");

        if (!orderOk)
            details.Add("Placeholder order changed.");

        if (!countOk)
            details.Add("Placeholder counts changed.");

        if (details.Count == 0)
            details.Add("All placeholders preserved exactly.");

        return Task.FromResult(
            new BenchmarkValidationResult
            {
                Id = Guid.NewGuid(),
                BenchmarkResultId = result.Id,
                Validator = Name,
                ValidationType = ValidationType,
                Passed = passed,
                Details = string.Join(" | ", details)
            });
    }

    private static List<string> ExtractPlaceholders(string text)
    {
        return
        [
            .. PlaceholderRegex().Matches(text).Select(x => x.Value)
        ];
    }

    [GeneratedRegex(@"\{\{.*?\}\}")]
    private static partial Regex PlaceholderRegex();
}