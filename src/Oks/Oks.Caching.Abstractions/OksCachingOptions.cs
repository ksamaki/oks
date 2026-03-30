namespace Oks.Caching.Abstractions;

public class OksCachingOptions
{
    public CacheProvider Provider { get; set; } = CacheProvider.Memory;

    public bool RepositoryCachingEnabled { get; set; } = true;

    public RedisCacheOptions Redis { get; set; } = new();

    public RepositoryQueryCacheScope RepositoryQueryCacheScope { get; set; } = RepositoryQueryCacheScope.ListOnly;

    public CacheEntryOptions DefaultEntryOptions { get; set; } = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
        SoftExpiration = TimeSpan.FromSeconds(30),
        Tags = Array.Empty<string>()
    };
}
