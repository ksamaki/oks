namespace Oks.Web.RateLimiting;

public sealed class OksRateLimitOptions
{
    /// <summary>
    /// RateLimit mekanizması global olarak aktif mi?
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Varsayılan dakikadaki maksimum istek sayısı.
    /// </summary>
    public int DefaultRequestsPerMinute { get; set; } = 60;
}