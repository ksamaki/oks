using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using Oks.Caching;
using Oks.Caching.Abstractions;

namespace Oks.Web.Filters;

public sealed class OksCustomCacheFilter(
    ICacheService cacheService,
    ICacheKeyGenerator keyGenerator,
    IOptions<OksCachingOptions> options) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (context.ActionDescriptor is not ControllerActionDescriptor descriptor)
        {
            await next();
            return;
        }

        ValidateUsage(descriptor.MethodInfo);

        var executor = new OksQueryCacheExecutor(cacheService, keyGenerator, options.Value);
        var arguments = context.ActionArguments.ToDictionary(x => x.Key, x => x.Value);
        var result = await executor.ExecuteAsync(
            descriptor.MethodInfo,
            arguments,
            async () =>
            {
                var executed = await next();
                return (executed.Result as ObjectResult)?.Value;
            },
            context.HttpContext.RequestAborted);

        if (descriptor.MethodInfo.GetCustomAttributes(typeof(OksCacheAttribute), true).Length > 0)
            context.Result = new ObjectResult(result);
    }

    private static void ValidateUsage(System.Reflection.MethodInfo method)
    {
        if (method.GetCustomAttributes(typeof(OksEntityCacheAttribute), true).Length > 0)
            throw new InvalidOperationException($"[OksEntityCache] sadece entity class seviyesinde kullanılabilir: {method.DeclaringType?.FullName}.{method.Name}");
    }
}
