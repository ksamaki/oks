using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Oks.Web.Filters;
using Oks.Web.Middleware;
using Oks.Web.RateLimiting;
using Oks.Web.Performance;

namespace Oks.Web.Extensions;

public static class OksWebServiceCollectionExtensions
{
    /// <summary>
    /// OKS UnitOfWork otomatik commit davranışını ekler.
    /// MVC action'larda filter, Minimal API endpoint'lerinde middleware ile request sonunda otomatik SaveChanges denenir.
    /// Değişiklik yoksa IUnitOfWork implementasyonu no-op dönebilir.
    /// </summary>
    public static IMvcBuilder AddOksUnitOfWork(this IMvcBuilder mvcBuilder)
    {
        // Filter'ı DI üzerinden kullanmak için kaydet
        mvcBuilder.Services.AddScoped<OksUnitOfWorkFilter>();
        mvcBuilder.Services.TryAddEnumerable(
            ServiceDescriptor.Transient<IStartupFilter, OksUnitOfWorkStartupFilter>());

        mvcBuilder.AddMvcOptions(options =>
        {
            options.Filters.AddService<OksUnitOfWorkFilter>();
        });

        return mvcBuilder;
    }

    /// <summary>
    /// Filter bunu yakalayıp StatusCode = (int)ResultStatus.Ok yapıyor
    /// JSON gövde aynen Result/DataResult yapısı
    /// </summary>
    public static IMvcBuilder AddOksResultWrapping(this IMvcBuilder mvcBuilder)
    {
        mvcBuilder.Services.AddScoped<OksResultFilter>();

        mvcBuilder.AddMvcOptions(options =>
        {
            options.Filters.AddService<OksResultFilter>();
        });

        return mvcBuilder;
    }

    /// <summary>
    /// OKS global exception middleware'ini servislere ekler.
    /// </summary>
    public static IServiceCollection AddOksExceptionHandling(this IServiceCollection services)
    {
        services.AddTransient<OksExceptionMiddleware>();
        return services;
    }

    /// <summary>
    /// Request pipeline'ına OKS exception middleware'ini ekler.
    /// </summary>
    public static IApplicationBuilder UseOksExceptionHandling(this IApplicationBuilder app)
            => app.UseMiddleware<OksExceptionMiddleware>();

    public static IMvcBuilder AddOksRateLimiting(
    this IMvcBuilder mvcBuilder,
    Action<OksRateLimitOptions>? configureOptions = null)
    {
        var services = mvcBuilder.Services;

        services.AddMemoryCache();

        if (configureOptions is not null)
        {
            services.Configure(configureOptions);
        }
        else
        {
            services.Configure<OksRateLimitOptions>(_ => { });
        }

        services.AddScoped<OksRateLimitFilter>();

        mvcBuilder.AddMvcOptions(options =>
        {
            options.Filters.AddService<OksRateLimitFilter>();
        });

        return mvcBuilder;
    }

    public static IMvcBuilder AddOksPerformance(
        this IMvcBuilder mvcBuilder,
        Action<OksPerformanceOptions>? configureOptions = null)
    {
        var services = mvcBuilder.Services;

        if (configureOptions is not null)
        {
            services.Configure(configureOptions);
        }
        else
        {
            services.Configure<OksPerformanceOptions>(_ => { });
        }

        services.AddScoped<OksPerformanceFilter>();

        mvcBuilder.AddMvcOptions(options =>
        {
            options.Filters.AddService<OksPerformanceFilter>();
        });

        return mvcBuilder;
    }

    /// <summary>
    /// HTTP isteklerinin OksLogRequest tablosuna loglanması için gerekli middleware'i DI'a ekler.
    /// </summary>
    public static IServiceCollection AddOksRequestLogging(this IServiceCollection services)
    {
        services.AddTransient<OksRequestLoggingMiddleware>();
        return services;
    }
}
