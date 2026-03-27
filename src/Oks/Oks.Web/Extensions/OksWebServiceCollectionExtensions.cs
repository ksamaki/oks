using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Oks.Persistence.Abstractions;
using Oks.Web.Filters;
using Oks.Web.Middleware;
using Oks.Web.RateLimiting;
using Oks.Web.Performance;
using Oks.Web.Users;

namespace Oks.Web.Extensions;

public static class OksWebServiceCollectionExtensions
{
    public static IServiceCollection AddOksCurrentUserProvider(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.Replace(ServiceDescriptor.Scoped<IOksUserProvider, HttpContextOksUserProvider>());
        return services;
    }

    /// <summary>
    /// OKS UnitOfWork otomatik commit davranışını ekler.
    /// MVC action'larda filter ile request sonunda otomatik SaveChanges denenir.
    /// Minimal API endpoint'leri için RouteGroupBuilder üstünde AddOksUnitOfWork() extension'ı kullanılmalıdır.
    /// Değişiklik yoksa IUnitOfWork implementasyonu no-op dönebilir.
    /// </summary>
    public static IMvcBuilder AddOksUnitOfWork(this IMvcBuilder mvcBuilder)
    {
        // Filter'ı DI üzerinden kullanmak için kaydet
        mvcBuilder.Services.AddScoped<OksUnitOfWorkFilter>();

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
        services.AddOksRateLimiting(configureOptions);
        services.AddScoped<OksRateLimitFilter>();
        services.AddScoped<OksMinimalApiRateLimitFilter>();

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
        services.AddOksPerformance(configureOptions);
        services.AddScoped<OksPerformanceFilter>();
        services.AddScoped<OksMinimalApiPerformanceFilter>();

        mvcBuilder.AddMvcOptions(options =>
        {
            options.Filters.AddService<OksPerformanceFilter>();
        });

        return mvcBuilder;
    }


    public static IMvcBuilder AddOksCustomCaching(this IMvcBuilder mvcBuilder)
    {
        mvcBuilder.Services.AddScoped<OksCustomCacheFilter>();

        mvcBuilder.AddMvcOptions(options =>
        {
            options.Filters.AddService<OksCustomCacheFilter>();
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

    public static IApplicationBuilder UseOksRequestLogging(this IApplicationBuilder app)
        => app.UseMiddleware<OksRequestLoggingMiddleware>();

    public static IServiceCollection AddOksRateLimiting(
        this IServiceCollection services,
        Action<OksRateLimitOptions>? configureOptions = null)
    {
        services.AddMemoryCache();

        if (configureOptions is not null)
            services.Configure(configureOptions);
        else
            services.Configure<OksRateLimitOptions>(_ => { });

        services.AddScoped<OksMinimalApiRateLimitFilter>();
        return services;
    }

    public static IServiceCollection AddOksCustomCaching(this IServiceCollection services)
    {
        services.AddScoped<OksMinimalApiCustomCacheFilter>();
        services.AddScoped<OksCustomCacheFilter>();
        return services;
    }

    public static IServiceCollection AddOksPerformance(
        this IServiceCollection services,
        Action<OksPerformanceOptions>? configureOptions = null)
    {
        if (configureOptions is not null)
            services.Configure(configureOptions);
        else
            services.Configure<OksPerformanceOptions>(_ => { });

        services.AddScoped<OksMinimalApiPerformanceFilter>();
        return services;
    }
}
