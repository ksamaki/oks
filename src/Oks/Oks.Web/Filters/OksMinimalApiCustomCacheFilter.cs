using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Oks.Caching.Abstractions;

namespace Oks.Web.Filters;

public sealed class OksMinimalApiCustomCacheFilter(
    ICacheService cacheService,
    ICacheKeyGenerator keyGenerator,
    IOptions<OksCachingOptions> options) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var endpoint = context.HttpContext.GetEndpoint();
        var method = endpoint?.Metadata.GetMetadata<MethodInfo>();
        if (method is null)
            return await next(context);

        ValidateUsage(method);

        var parameters = method.GetParameters();
        var args = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < parameters.Length && i < context.Arguments.Count; i++)
            args[parameters[i].Name ?? $"arg{i}"] = context.Arguments[i];

        var executor = new OksQueryCacheExecutor(cacheService, keyGenerator, options.Value);
        return await executor.ExecuteAsync(method, args, () => next(context).AsTask(), context.HttpContext.RequestAborted);
    }

    private static void ValidateUsage(MethodInfo method)
    {
        if (method.GetCustomAttributes(typeof(OksEntityCacheAttribute), true).Length > 0)
            throw new InvalidOperationException($"[OksEntityCache] sadece entity class seviyesinde kullanılabilir: {method.DeclaringType?.FullName}.{method.Name}");
    }
}
