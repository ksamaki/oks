namespace Oks.Web.Performance;

public sealed class OksPerformanceException : Exception
{
    public long ElapsedMs { get; }
    public int ThresholdMs { get; }

    public OksPerformanceException(string message, long elapsedMs, int thresholdMs)
        : base(message)
    {
        ElapsedMs = elapsedMs;
        ThresholdMs = thresholdMs;
    }
}