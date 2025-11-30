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

        // Her request için bir WriteTracker
        services.AddScoped<WriteTracker>();

        // Unit of Work
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();

        // Generic repository implementasyonları
        services.AddScoped(typeof(IReadRepository<,>), typeof(EfReadRepository<,>));
        services.AddScoped(typeof(IWriteRepository<,>), typeof(EfWriteRepository<,>));

        return services;
    }
}
