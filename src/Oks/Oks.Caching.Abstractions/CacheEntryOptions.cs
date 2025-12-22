using Microsoft.Extensions.Caching.Memory;

namespace Oks.Caching.Abstractions;

public class CacheEntryOptions
{
    public TimeSpan? AbsoluteExpirationRelativeToNow { get; set; }

    public TimeSpan? SlidingExpiration { get; set; }

    public TimeSpan? SoftExpiration { get; set; }

    public string Version { get; set; } = "v1";

    public CacheItemPriority Priority { get; set; } = CacheItemPriority.Normal;

    public string[] Tags { get; set; } = Array.Empty<string>();
}
