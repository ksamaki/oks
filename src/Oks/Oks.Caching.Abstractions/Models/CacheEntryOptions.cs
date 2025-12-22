namespace Oks.Caching.Abstractions.Models;

public sealed class CacheEntryOptions
{
    public TimeSpan? AbsoluteExpirationRelativeToNow { get; set; }

    public TimeSpan? SlidingExpiration { get; set; }

    public TimeSpan? SoftTtl { get; set; }

    public string? Version { get; set; }

    public string[] Tags { get; set; } = Array.Empty<string>();
}
