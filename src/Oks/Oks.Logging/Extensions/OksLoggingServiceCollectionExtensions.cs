using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Oks.Logging.Abstractions.Interfaces;
using Oks.Logging.Writers;

namespace Oks.Logging.Extensions;

public static class OksLoggingServiceCollectionExtensions
{
    public static IServiceCollection AddOksLogging<TDbContext>(this IServiceCollection services)
        where TDbContext : DbContext
    {
        services.AddScoped<IOksLogWriter, EfCoreOksLogWriter<TDbContext>>();
        return services;
    }
}