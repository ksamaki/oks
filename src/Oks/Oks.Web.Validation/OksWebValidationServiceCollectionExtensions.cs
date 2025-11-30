using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
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
        // Eğer assembly verilmezse, entry assembly'i kullanmayı deneyelim
        if (validatorAssemblies is null || validatorAssemblies.Length == 0)
        {
            var entry = Assembly.GetEntryAssembly();
            if (entry is not null)
            {
                validatorAssemblies = new[] { entry };
            }
        }

        // FluentValidation validator'larını DI'ya ekle
        if (validatorAssemblies is not null && validatorAssemblies.Length > 0)
        {
            mvcBuilder.Services.AddValidatorsFromAssemblies(validatorAssemblies);
        }

        // Validation filtresini kaydet
        mvcBuilder.Services.AddScoped<OksValidationFilter>();

        mvcBuilder.AddMvcOptions(options =>
        {
            // Her requestte action'dan önce bu filter çalışacak
            options.Filters.AddService<OksValidationFilter>();
        });

        return mvcBuilder;
    }
}