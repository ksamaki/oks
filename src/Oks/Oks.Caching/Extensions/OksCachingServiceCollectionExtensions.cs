namespace Oks.Caching.Extensions;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Oks.Caching.Abstractions.Interfaces;
using Oks.Caching.Builders;
using Oks.Caching.Options;
using Oks.Caching.Serializers;
using Oks.Caching.Services;

public static class OksCachingServiceCollectionExtensions
{
    public static IServiceCollection AddOksCaching(this IServiceCollection services, Action<OksCachingOptions>? configureOptions = null)
    {
        services.AddMemoryCache();

        services.TryAddSingleton<ICacheSerializer, DefaultCacheSerializer>();
        services.TryAddSingleton<ICacheKeyBuilder, DefaultCacheKeyBuilder>();
        services.TryAddSingleton<ICacheService, CacheService>();

        if (configureOptions is not null)
        {
            services.Configure(configureOptions);
        }
        else
        {
            services.Configure<OksCachingOptions>(_ => { });
        }

        return services;
    }
}
