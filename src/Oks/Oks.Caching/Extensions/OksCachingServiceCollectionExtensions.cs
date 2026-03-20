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

        var options = new OksCachingOptions();
        configure?.Invoke(options);

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
}
