using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Oks.Caching.Abstractions;

namespace Oks.Web.Filters;

public sealed class OksCustomCacheFilter(
    ICacheService cacheService,
    ICacheKeyBuilder keyBuilder,
    ICacheEntityNameResolver entityNameResolver) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var attribute = ResolveAttribute(context);
        if (attribute is null)
        {
            await next();
            return;
        }

        var key = ResolveKey(context, attribute);
        var tags = ResolveTags(attribute, context);

        if (attribute.Evict || !HttpMethods.IsGet(context.HttpContext.Request.Method))
        {
            await EvictAsync(tags, key, context.HttpContext.RequestAborted);
            await next();
            return;
        }

        var cached = await cacheService.GetAsync<object>(key, context.HttpContext.RequestAborted);
        if (cached is not null)
        {
            context.Result = new ObjectResult(cached);
            return;
        }

        var executedContext = await next();
        if (executedContext.Result is not ObjectResult objectResult)
            return;

        var options = new CacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(attribute.DurationSeconds),
            Tags = tags
        };

        await cacheService.SetAsync(key, objectResult.Value, options, context.HttpContext.RequestAborted);
    }

    private static CustomCacheAttribute? ResolveAttribute(ActionExecutingContext context)
        => context.ActionDescriptor.EndpointMetadata.OfType<CustomCacheAttribute>().FirstOrDefault();

    private string[] ResolveTags(CustomCacheAttribute attribute, ActionExecutingContext context)
    {
        if (attribute.Tags.Length > 0)
            return attribute.Tags;

        var tags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var argument in context.ActionArguments.Values.Where(v => v is not null))
        {
            foreach (var entityName in entityNameResolver.ResolveFromType(argument!.GetType()))
                tags.Add(entityName);
        }

        if (context.ActionDescriptor is ControllerActionDescriptor actionDescriptor)
        {
            foreach (var entityName in entityNameResolver.ResolveFromType(actionDescriptor.MethodInfo.ReturnType))
                tags.Add(entityName);
        }

        return tags.ToArray();
    }

    private CacheKey ResolveKey(ActionExecutingContext context, CustomCacheAttribute attribute)
    {
        if (!string.IsNullOrWhiteSpace(attribute.KeyTemplate))
            return keyBuilder.FromTemplate(attribute.KeyTemplate, context.ActionArguments);

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
