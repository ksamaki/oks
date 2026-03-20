using Oks.Caching.Abstractions;

namespace Oks.Caching.Extensions;

public static class OksCachingOptionsExtensions
{
    public static OksCachingOptions UseDistributedCache(this OksCachingOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        options.Provider = CacheProvider.Distributed;
        return options;
    }

    public static OksCachingOptions UseMemoryCache(this OksCachingOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        options.Provider = CacheProvider.Memory;
        return options;
    }

    public static OksCachingOptions AddReadRepositoryCaching(this OksCachingOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        options.RepositoryCachingEnabled = true;
        return options;
    }
}
