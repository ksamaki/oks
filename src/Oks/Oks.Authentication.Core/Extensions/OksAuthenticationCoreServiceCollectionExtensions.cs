using Microsoft.Extensions.DependencyInjection;
using Oks.Authentication.Abstractions.Contracts;
using Oks.Authentication.Core.Services;

namespace Oks.Authentication.Core.Extensions;

public static class OksAuthenticationCoreServiceCollectionExtensions
{
    public static IServiceCollection AddOksAuthenticationCore(this IServiceCollection services)
    {
        services.AddScoped<IAuthenticationService, DefaultAuthenticationService>();
        services.AddSingleton<IAuthSecurityEventPublisher, NoOpAuthSecurityEventPublisher>();
        return services;
    }
}
