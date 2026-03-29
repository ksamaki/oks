using Microsoft.Extensions.DependencyInjection;
using Oks.Authentication.OpenIddict.Options;
using Oks.Authentication.OpenIddict.Services;

namespace Oks.Authentication.OpenIddict.Extensions;

public static class OksOpenIddictServiceCollectionExtensions
{
    public static IServiceCollection AddOksOpenIddict<TConfigurator>(
        this IServiceCollection services,
        Action<OksOpenIddictOptions>? configure = null)
        where TConfigurator : class, IOksOpenIddictConfigurator
    {
        var options = new OksOpenIddictOptions();
        configure?.Invoke(options);

        services.AddSingleton(options);
        services.AddSingleton<IOksOpenIddictConfigurator, TConfigurator>();

        using var provider = services.BuildServiceProvider();
        var configurator = provider.GetRequiredService<IOksOpenIddictConfigurator>();
        configurator.Configure(services, options);

        return services;
    }
}
