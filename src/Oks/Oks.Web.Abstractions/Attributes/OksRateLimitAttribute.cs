namespace Oks.Web.Abstractions.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
public sealed class OksRateLimitAttribute : Attribute
{
    /// <summary>
    /// Dakikadaki maksimum istek sayısı.
    /// </summary>
    public int RequestsPerMinute { get; }

    public OksRateLimitAttribute(int requestsPerMinute)
    {
        if (requestsPerMinute <= 0)
            throw new ArgumentOutOfRangeException(nameof(requestsPerMinute), "RequestsPerMinute must be > 0.");

        RequestsPerMinute = requestsPerMinute;
    }
}