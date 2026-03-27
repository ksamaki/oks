using Oks.Caching.Abstractions;

namespace Oks.Caching;

public sealed class CacheManager(ICacheService cacheService) : ICacheManager
{
    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        => cacheService.GetAsync<T>(ToCacheKey(key), cancellationToken);

    public Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, CacheEntryOptions? options = null, CancellationToken cancellationToken = default)
        => cacheService.GetOrAddAsync(ToCacheKey(key), factory, options, cancellationToken);

    public Task SetAsync<T>(string key, T value, CacheEntryOptions? options = null, CancellationToken cancellationToken = default)
        => cacheService.SetAsync(ToCacheKey(key), value, options, cancellationToken);

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        => cacheService.RemoveAsync(ToCacheKey(key), cancellationToken);

    public Task RemoveByTagAsync(string tag, CancellationToken cancellationToken = default)
        => cacheService.RemoveByTagAsync(tag, cancellationToken);

    private static CacheKey ToCacheKey(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        return new CacheKey(new[] { key });
    }
}
