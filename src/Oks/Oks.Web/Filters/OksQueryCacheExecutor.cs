using System.Reflection;
using Oks.Caching.Abstractions;

namespace Oks.Web.Filters;

internal sealed class OksQueryCacheExecutor(ICacheService cacheService, ICacheKeyGenerator keyGenerator, OksCachingOptions options)
{
    public async Task<object?> ExecuteAsync(
        MethodInfo method,
        IReadOnlyDictionary<string, object?> arguments,
        Func<Task<object?>> next,
        CancellationToken cancellationToken)
    {
        var invalidate = method.GetCustomAttributes<OksCacheInvalidateAttribute>(true).ToArray();
        if (invalidate.Length > 0)
        {
            foreach (var attribute in invalidate)
            {
                var tags = keyGenerator.GenerateTags(attribute.Tags, method, arguments);
                foreach (var tag in tags)
                    await cacheService.RemoveByTagAsync(tag, cancellationToken);
            }
        }

        var cache = method.GetCustomAttribute<OksCacheAttribute>(true);
        if (cache is null)
            return await next();

        var keyValue = keyGenerator.GenerateKey(cache.Key, method, arguments);
        var cacheKey = new CacheKey([keyValue]);

        var cached = await cacheService.GetAsync<object>(cacheKey, cancellationToken);
        if (cached is not null)
            return cached;

        var result = await next();
        if (result is null && !cache.CacheEmptyResult)
            return result;

        var ttlSeconds = cache.TtlSeconds ?? (int?)options.DefaultEntryOptions.AbsoluteExpirationRelativeToNow?.TotalSeconds;
        var entry = new CacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = ttlSeconds.HasValue ? TimeSpan.FromSeconds(ttlSeconds.Value) : options.DefaultEntryOptions.AbsoluteExpirationRelativeToNow,
            SlidingExpiration = options.DefaultEntryOptions.SlidingExpiration,
            SoftExpiration = options.DefaultEntryOptions.SoftExpiration,
            Tags = keyGenerator.GenerateTags(cache.Tags, method, arguments).ToArray()
        };

        if (cache.StampedeProtection)
        {
            await cacheService.GetOrAddAsync(cacheKey, () => Task.FromResult(result), entry, cancellationToken);
            return result;
        }

        await cacheService.SetAsync(cacheKey, result, entry, cancellationToken);
        return result;
    }
}
