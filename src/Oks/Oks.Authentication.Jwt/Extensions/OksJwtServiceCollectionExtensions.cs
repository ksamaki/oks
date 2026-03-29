using Microsoft.Extensions.DependencyInjection;
using Oks.Authentication.Abstractions.Contracts;
using Oks.Authentication.Jwt.Options;
using Oks.Authentication.Jwt.Services;

namespace Oks.Authentication.Jwt.Extensions;

public static class OksJwtServiceCollectionExtensions
{
    public static IServiceCollection AddOksJwt(this IServiceCollection services, Action<OksJwtOptions> configure)
    {
        services.Configure(configure);
        services.AddSingleton<ITokenIssuer, JwtTokenIssuer>();
        return services;
    }
}
