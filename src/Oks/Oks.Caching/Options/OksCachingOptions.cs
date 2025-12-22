namespace Oks.Caching.Options;

using Oks.Caching.Abstractions.Models;

public sealed class OksCachingOptions
{
    public OksCachingProvider Provider { get; set; } = OksCachingProvider.Memory;

    public CacheEntryOptions DefaultEntryOptions { get; set; } = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
    };
}

public enum OksCachingProvider
{
    Memory = 0,
    Distributed = 1
}
