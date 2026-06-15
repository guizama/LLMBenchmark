using System.Text.RegularExpressions;
using LLMBenchmark.Api.Features.Benchmark.Enums;
using LLMBenchmark.Api.Features.Benchmark.Models.Benchmark;
using LLMBenchmark.Api.Features.Benchmark.Validators.Contracts;
using LLMBenchmark.Api.Features.Benchmark.Validators.Models;

namespace LLMBenchmark.Api.Features.Benchmark.Validators.Deterministic;

public sealed partial class LinkValidator : IBenchmarkValidator
{
    public string Name => "LinkValidator";
    public ValidatorType ValidationType => ValidatorType.Deterministic;

    public Task<BenchmarkValidationResult> ValidateAsync(BenchmarkScenario scenario, BenchmarkResult result, CancellationToken cancellationToken = default)
    {
        var inputLinks = ExtractLinks(result.InputPrompt);
        var outputLinks = ExtractLinks(result.Output);

        var missing = inputLinks.Except(outputLinks).ToList();
        var extra = outputLinks.Except(inputLinks).ToList();

        var orderOk = inputLinks.SequenceEqual(outputLinks);

        var inputCounts = inputLinks.GroupBy(x => x).ToDictionary(x => x.Key, x => x.Count());

        var outputCounts = outputLinks.GroupBy(x => x).ToDictionary(x => x.Key, x => x.Count());

        var countOk =
            inputCounts.Count == outputCounts.Count &&
            inputCounts.All(x =>
                outputCounts.TryGetValue(
                    x.Key,
                    out var count) &&
                count == x.Value);

        var passed = missing.Count == 0 && extra.Count == 0 && orderOk && countOk;

        var details = new List<string>();

        if (missing.Count > 0)
            details.Add($"Missing: [{string.Join(", ", missing)}]");

        if (extra.Count > 0)
            details.Add($"Extra: [{string.Join(", ", extra)}]");

        if (!orderOk)
            details.Add("URL order changed.");

        if (!countOk)
            details.Add("URL counts changed.");

        if (details.Count == 0)
            details.Add("All URLs preserved exactly.");

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

    private static List<string> ExtractLinks(string text)
    {
        return
        [
            .. UrlRegex()
                .Matches(text)
                .Select(x => x.Value)
        ];
    }

    [GeneratedRegex(@"(https?:\/\/|www\.)[^\s]+")]
    private static partial Regex UrlRegex();
}