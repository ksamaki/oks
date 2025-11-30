namespace Oks.Web.Abstractions.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
public sealed class OksPerformanceAttribute : Attribute
{
    /// <summary>
    /// Bu endpoint için milisaniye cinsinden özel threshold.
    /// </summary>
    public int ThresholdMs { get; }

    public OksPerformanceAttribute(int thresholdMs)
    {
        if (thresholdMs <= 0)
            throw new ArgumentOutOfRangeException(nameof(thresholdMs), "Threshold must be > 0.");

        ThresholdMs = thresholdMs;
    }
}