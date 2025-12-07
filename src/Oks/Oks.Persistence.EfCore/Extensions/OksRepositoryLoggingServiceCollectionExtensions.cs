using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Oks.Persistence.EfCore.Options;

namespace Oks.Persistence.EfCore.Extensions;

public static class OksRepositoryLoggingServiceCollectionExtensions
{
    public static IServiceCollection AddOksRepositoryLogging(
        this IServiceCollection services,
        Action<OksRepositoryLoggingOptions>? configure = null)
    {
        if (configure is not null)
        {
            services.Configure(configure);
        }
        else
        {
            // Default instance (Enabled = false kalır)
            services.Configure<OksRepositoryLoggingOptions>(_ => { });
        }

        return services;
    }
}