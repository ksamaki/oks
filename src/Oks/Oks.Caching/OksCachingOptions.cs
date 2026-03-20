using Oks.Caching.Abstractions;

namespace Oks.Caching;

public class OksCachingOptions
{
    public CacheProvider Provider { get; set; } = CacheProvider.Memory;

    public bool RepositoryCachingEnabled { get; set; } = true;

    public CacheEntryOptions DefaultEntryOptions { get; set; } = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
        SoftExpiration = TimeSpan.FromSeconds(30),
        Tags = Array.Empty<string>()
    };
}
