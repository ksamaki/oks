using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Oks.Authentication.Abstractions.Contracts;
using Oks.Authentication.EntityFrameworkCore.Options;
using Oks.Authentication.EntityFrameworkCore.Persistence;
using Oks.Authentication.EntityFrameworkCore.Services;

namespace Oks.Authentication.EntityFrameworkCore.Extensions;

public static class OksAuthenticationEntityFrameworkServiceCollectionExtensions
{
    public static IServiceCollection AddOksAuthenticationEntityFramework<TDbContext>(
        this IServiceCollection services,
        Action<OksAuthenticationEfCoreOptions>? configureOptions = null)
        where TDbContext : OksAuthenticationDbContext
    {
        services.Configure(configureOptions ?? (_ => { }));
        services.AddScoped(sp => sp.GetRequiredService<IOptions<OksAuthenticationEfCoreOptions>>().Value);

        services.AddScoped<IRefreshTokenStore, EfCoreRefreshTokenStore>();
        services.AddScoped<ISecretHasher, Sha256SecretHasher>();
        services.AddScoped<IAuthSecurityEventPublisher, EfCoreAuthSecurityEventPublisher>();
        return services;
    }

    public static async Task UseOksAuthenticationEntityFrameworkAsync(this IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<OksAuthenticationDbContext>();
        var options = scope.ServiceProvider.GetRequiredService<IOptions<OksAuthenticationEfCoreOptions>>().Value;

        if (options.AutoMigrate)
            await dbContext.Database.MigrateAsync(cancellationToken);
    }
}
