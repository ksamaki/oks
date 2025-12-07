using Microsoft.Extensions.DependencyInjection;
using Oks.Persistence.EfCore.Options;

namespace Oks.Persistence.EfCore.Extensions;

public static class OksAuditLoggingServiceCollectionExtensions
{
    public static IServiceCollection AddOksAuditLogging(
        this IServiceCollection services,
        Action<OksAuditLoggingOptions>? configure = null)
    {
        if (configure is not null)
        {
            services.Configure(configure);
        }
        else
        {
            services.Configure<OksAuditLoggingOptions>(_ => { });
        }

        return services;
    }
}