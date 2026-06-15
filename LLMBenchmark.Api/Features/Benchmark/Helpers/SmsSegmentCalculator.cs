namespace LLMBenchmark.Api.Features.Benchmark.Helpers;

public static class SmsSegmentCalculator
{
    public static int Calculate(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return 0;
        }

        var isUnicode = text.Any(c => c > 127);

        var singleLimit = isUnicode ? 70 : 160;
        var multiLimit = isUnicode ? 67 : 153;

        if (text.Length <= singleLimit)
        {
            return 1;
        }

        return (int)Math.Ceiling(
            text.Length / (double)multiLimit);
    }
}