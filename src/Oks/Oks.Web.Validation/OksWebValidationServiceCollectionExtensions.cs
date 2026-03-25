using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Oks.Web.Validation.Behaviors;
using Oks.Web.Validation.Filters;
using System.Reflection;

namespace Oks.Web.Validation;

public static class OksWebValidationServiceCollectionExtensions
{
    /// <summary>
    /// FluentValidation validator'larını DI'a ekler ve
    /// OKS validation filtresini MVC pipeline'ına bağlar.
    /// </summary>
    public static IMvcBuilder AddOksFluentValidation(
        this IMvcBuilder mvcBuilder,
        params Assembly[] validatorAssemblies)
    {
        if (validatorAssemblies is null || validatorAssemblies.Length == 0)
        {
            var entry = Assembly.GetEntryAssembly();
            if (entry is not null)
            {
                validatorAssemblies = new[] { entry };
            }
        }

        if (validatorAssemblies is not null && validatorAssemblies.Length > 0)
        {
            mvcBuilder.Services.AddValidatorsFromAssemblies(validatorAssemblies);
        }

        mvcBuilder.Services.AddScoped<OksValidationFilter>();
        mvcBuilder.Services.AddScoped<OksMinimalApiValidationFilter>();

        mvcBuilder.AddMvcOptions(options =>
        {
            options.Filters.AddService<OksValidationFilter>();
        });

        return mvcBuilder;
    }

    public static IServiceCollection AddOksMediatRValidationBehavior(this IServiceCollection services)
    {
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(OksValidationBehavior<,>));
        return services;
    }
}
