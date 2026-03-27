using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Oks.Caching.Abstractions;
using Oks.Caching.Repositories;
using Oks.Caching.Tags;
using Oks.Persistence.Abstractions.Repositories;

namespace Oks.Caching.Extensions;

public static class OksCachingServiceCollectionExtensions
{
    public static IServiceCollection AddOksCaching(
        this IServiceCollection services,
        Action<OksCachingOptions>? configure = null)
    {
        services.AddMemoryCache();
        services.TryAddSingleton<ICacheSerializer, DefaultCacheSerializer>();
        services.TryAddSingleton<ICacheKeyBuilder, CacheKeyBuilder>();
        services.TryAddSingleton<ICacheTagIndex, InMemoryCacheTagIndex>();
        services.TryAddSingleton<ICacheService, CacheService>();
        services.TryAddSingleton<ICacheManager, CacheManager>();
        services.TryAddSingleton<ICacheEntityNameResolver, ConventionCacheEntityNameResolver>();

        var options = new OksCachingOptions();
        configure?.Invoke(options);

        ConfigureDistributedProvider(services, options);

        services.TryAddScoped(typeof(CachedReadRepository<,>));
        services.TryAddScoped(typeof(CacheEvictingWriteRepository<,>));

        if (options.RepositoryCachingEnabled)
        {
            services.Replace(ServiceDescriptor.Scoped(typeof(IReadRepository<,>), typeof(CachedReadRepository<,>)));
            services.Replace(ServiceDescriptor.Scoped(typeof(IWriteRepository<,>), typeof(CacheEvictingWriteRepository<,>)));
        }

        services.Configure<OksCachingOptions>(configuredOptions =>
        {
            configuredOptions.Provider = options.Provider;
            configuredOptions.RepositoryCachingEnabled = options.RepositoryCachingEnabled;
            configuredOptions.RepositoryQueryCacheScope = options.RepositoryQueryCacheScope;
            configuredOptions.Redis = new RedisCacheOptions
            {
                Enabled = options.Redis.Enabled,
                Configuration = options.Redis.Configuration,
                InstanceName = options.Redis.InstanceName
            };
            configuredOptions.DefaultEntryOptions = new CacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = options.DefaultEntryOptions.AbsoluteExpirationRelativeToNow,
                SlidingExpiration = options.DefaultEntryOptions.SlidingExpiration,
                SoftExpiration = options.DefaultEntryOptions.SoftExpiration,
                Priority = options.DefaultEntryOptions.Priority,
                Tags = options.DefaultEntryOptions.Tags.ToArray(),
                Version = options.DefaultEntryOptions.Version
            };
        });

        return services;
    }

    public static IServiceCollection AddOksCachingWithRepositories(
        this IServiceCollection services,
        Action<OksCachingOptions>? configure = null)
    {
        return services.AddOksCaching(configure);
    }

    private static void ConfigureDistributedProvider(IServiceCollection services, OksCachingOptions options)
    {
        if (options.Provider != CacheProvider.Distributed)
            return;

        if (options.Redis.ConfigureServices is not null)
        {
            options.Redis.ConfigureServices(services);
            return;
        }

        if (options.Redis.Enabled)
        {
            services.AddStackExchangeRedisCache(redisOptions =>
            {
                redisOptions.Configuration = options.Redis.Configuration;
                redisOptions.InstanceName = options.Redis.InstanceName;
            });
            return;
        }

        services.AddDistributedMemoryCache();
    }
}
