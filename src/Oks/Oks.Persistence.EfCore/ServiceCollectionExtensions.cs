using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Oks.Persistence.Abstractions.Caching;
using Oks.Persistence.Abstractions.Repositories;
using Oks.Persistence.EfCore.Caching;
using Oks.Persistence.EfCore.Options;
using Oks.Persistence.EfCore.Repositories;

namespace Oks.Persistence.EfCore;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOksEfCore<TDbContext>(
        this IServiceCollection services)
        where TDbContext : DbContext
    {
        // Uygulama kendi AddDbContext<WaitMeDbContext> vs. ekleyecek.
        // Biz burada DbContext base türü olarak TDbContext'i kullan diyoruz.
        services.AddScoped<DbContext, TDbContext>();

        // Her request için bir WriteTracker
        services.AddScoped<WriteTracker>();

        // Unit of Work
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();

        // Generic repository implementasyonları
        services.AddScoped(typeof(IReadRepository<,>), typeof(EfReadRepository<,>));
        services.AddScoped(typeof(IWriteRepository<,>), typeof(EfWriteRepository<,>));

        // Default cache options to avoid missing options when caching is enabled later.
        services.AddOptions<OksRepositoryCacheOptions>();

        return services;
    }

    /// <summary>
    /// IReadRepository çağrıları için bellek içi cache katmanını aktif eder.
    /// </summary>
    /// <param name="services">DI container</param>
    /// <param name="configure">
    /// Varsayılan süreleri vs. değiştirmek için <see cref="OksRepositoryCacheOptions"/> konfigurasyonu.
    /// </param>
    /// <returns></returns>
    public static IServiceCollection AddOksRepositoryCache(
        this IServiceCollection services,
        Action<OksRepositoryCacheOptions>? configure = null)
    {
        services.AddMemoryCache();

        if (configure is null)
        {
            services.Configure<OksRepositoryCacheOptions>(_ => { });
        }
        else
        {
            services.Configure(configure);
        }

        services.AddSingleton<RepositoryCacheTokenProvider>();
        services.AddSingleton<IRepositoryCacheTokenProvider>(sp => sp.GetRequiredService<RepositoryCacheTokenProvider>());
        services.AddSingleton<IRepositoryCacheInvalidator>(sp => sp.GetRequiredService<RepositoryCacheTokenProvider>());

        services.AddScoped(typeof(IReadRepository<,>), typeof(CachedEfReadRepository<,>));

        return services;
    }
}
