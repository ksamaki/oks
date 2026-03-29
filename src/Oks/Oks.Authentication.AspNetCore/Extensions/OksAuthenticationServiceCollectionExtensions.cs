using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Oks.Authentication.Abstractions.Contracts;
using Oks.Authentication.AspNetCore.Authorization;
using Oks.Authentication.Core.Services;

namespace Oks.Authentication.AspNetCore.Extensions;

public static class OksAuthenticationServiceCollectionExtensions
{
    public static IServiceCollection AddOksAuthentication(this IServiceCollection services)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();

        services.AddAuthorization();
        services.AddScoped<IAuthenticationService, DefaultAuthenticationService>();
        services.AddSingleton<IAuthorizationHandler, OksPermissionAuthorizationHandler>();
        services.AddSingleton<IAuthSecurityEventPublisher, NoOpAuthSecurityEventPublisher>();
        return services;
    }

    public static IServiceCollection AddOksPermissionPolicy(this IServiceCollection services, string policyName, string permission)
    {
        services.AddAuthorizationBuilder()
            .AddPolicy(policyName, policy => policy.Requirements.Add(new OksPermissionRequirement(permission)));

        return services;
    }

    public static IApplicationBuilder UseOksAuthentication(this IApplicationBuilder app)
    {
        app.UseAuthentication();
        app.UseAuthorization();
        return app;
    }
}
