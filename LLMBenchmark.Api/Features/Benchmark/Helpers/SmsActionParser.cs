using LLMBenchmark.Api.Features.Benchmark.Enums;

namespace LLMBenchmark.Api.Features.Benchmark.Helpers;

public static class SmsActionParser
{
    public static bool TryParse(string? value, out SmsAction action)
    {
        action = default;

        if (string.IsNullOrWhiteSpace(value))
            return false;

        var normalized = value.Trim()
                .Replace("sms.", "", StringComparison.OrdinalIgnoreCase)
                .Replace("-", "", StringComparison.OrdinalIgnoreCase);

        return Enum.TryParse(normalized, ignoreCase: true, out action);
    }
}