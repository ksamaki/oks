using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Oks.Persistence.Abstractions.Repositories;
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

        // Unit of Work
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();

        // Generic repository implementasyonları
        services.AddKeyedScoped(typeof(IReadRepository<,>), "base", typeof(EfReadRepository<,>));
        services.AddKeyedScoped(typeof(IWriteRepository<,>), "base", typeof(EfWriteRepository<,>));

        // Varsayılan kayıt, keyed "base" implementasyonunu kullanır. Cache dekoratörü
        // gibi eklentiler bu default kaydı Replace ile sarabilir.
        services.AddScoped(typeof(IReadRepository<,>), sp =>
            sp.GetRequiredKeyedService(typeof(IReadRepository<,>), "base"));

        services.AddScoped(typeof(IWriteRepository<,>), sp =>
            sp.GetRequiredKeyedService(typeof(IWriteRepository<,>), "base"));

        return services;
    }
}
