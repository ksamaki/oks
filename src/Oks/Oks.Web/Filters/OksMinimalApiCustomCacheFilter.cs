using System.Reflection;
using Oks.Caching.Abstractions;

namespace Oks.Web.Filters;

public sealed class OksMinimalApiCustomCacheFilter(
    ICacheService cacheService,
    ICacheKeyBuilder keyBuilder,
    ICacheEntityNameResolver entityNameResolver) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var endpoint = context.HttpContext.GetEndpoint();
        var attribute = endpoint?.Metadata.GetMetadata<CustomCacheAttribute>();
        if (attribute is null)
            return await next(context);

        var key = ResolveKey(context, attribute);
        var tags = ResolveTags(attribute, context, endpoint);

        if (attribute.Evict || !HttpMethods.IsGet(context.HttpContext.Request.Method))
        {
            await EvictAsync(tags, key, context.HttpContext.RequestAborted);
            return await next(context);
        }

        var cached = await cacheService.GetAsync<object>(key, context.HttpContext.RequestAborted);
        if (cached is not null)
            return cached;

        var result = await next(context);

        var options = new CacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(attribute.DurationSeconds),
            Tags = tags
        };

        await cacheService.SetAsync(key, result, options, context.HttpContext.RequestAborted);
        return result;
    }

    private string[] ResolveTags(CustomCacheAttribute attribute, EndpointFilterInvocationContext context, Endpoint? endpoint)
    {
        if (attribute.Tags.Length > 0)
            return attribute.Tags;

        var tags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var argument in context.Arguments.Where(a => a is not null))
        {
            foreach (var entityName in entityNameResolver.ResolveFromType(argument!.GetType()))
                tags.Add(entityName);
        }

        var methodInfo = endpoint?.Metadata.GetMetadata<MethodInfo>();
        if (methodInfo is not null)
        {
            foreach (var entityName in entityNameResolver.ResolveFromType(methodInfo.ReturnType))
                tags.Add(entityName);
        }

        return tags.ToArray();
    }

    private CacheKey ResolveKey(EndpointFilterInvocationContext context, CustomCacheAttribute attribute)
    {
        if (!string.IsNullOrWhiteSpace(attribute.KeyTemplate))
            return keyBuilder.FromTemplate(attribute.KeyTemplate, context.Arguments);

        return new CacheKey(new[]
        {
            "http",
            context.HttpContext.Request.Method,
            context.HttpContext.Request.Path.Value ?? "/",
            context.HttpContext.Request.QueryString.Value ?? string.Empty
        });
    }

    private async Task EvictAsync(IEnumerable<string> tags, CacheKey key, CancellationToken cancellationToken)
    {
        await cacheService.RemoveAsync(key, cancellationToken);

        foreach (var tag in tags)
        {
            await cacheService.RemoveByTagAsync(tag, cancellationToken);
        }
    }
}
