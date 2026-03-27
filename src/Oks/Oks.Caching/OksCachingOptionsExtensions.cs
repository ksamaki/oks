using Microsoft.Extensions.DependencyInjection;
using Oks.Caching.Abstractions;

namespace Oks.Caching.Extensions;

public static class OksCachingOptionsExtensions
{
    public static OksCachingOptions UseDistributedCache(
        this OksCachingOptions options,
        Action<IServiceCollection>? configureProvider = null)
    {
        ArgumentNullException.ThrowIfNull(options);
        options.Provider = CacheProvider.Distributed;

        if (configureProvider is not null)
            options.Redis.ConfigureServices = configureProvider;

        return options;
    }

    public static OksCachingOptions UseMemoryCache(this OksCachingOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        options.Provider = CacheProvider.Memory;
        return options;
    }

    public static OksCachingOptions UseRedis(this OksCachingOptions options, string configuration, string instanceName = "oks:")
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(configuration);

        options.Provider = CacheProvider.Distributed;
        options.Redis.Enabled = true;
        options.Redis.Configuration = configuration;
        options.Redis.InstanceName = instanceName;
        options.Redis.ConfigureServices = services =>
        {
            services.AddStackExchangeRedisCache(redisOptions =>
            {
                redisOptions.Configuration = configuration;
                redisOptions.InstanceName = instanceName;
            });
        };

        return options;
    }

    public static OksCachingOptions CacheAllRepositoryQueries(this OksCachingOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        options.RepositoryQueryCacheScope = RepositoryQueryCacheScope.All;
        return options;
    }

    public static OksCachingOptions CacheOnlyRepositoryListQueries(this OksCachingOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        options.RepositoryQueryCacheScope = RepositoryQueryCacheScope.ListOnly;
        return options;
    }

    public static OksCachingOptions AddReadRepositoryCaching(this OksCachingOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        options.RepositoryCachingEnabled = true;
        return options;
    }
}
