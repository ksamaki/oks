namespace Oks.Caching.Abstractions.Interfaces;

using Oks.Caching.Abstractions.Models;

public interface ICacheService
{
    Task<T?> GetAsync<T>(CacheKey key, CancellationToken cancellationToken = default);

    Task SetAsync<T>(CacheKey key, T value, CacheEntryOptions? options = null, CancellationToken cancellationToken = default);

    Task RemoveAsync(CacheKey key, CancellationToken cancellationToken = default);

    Task RemoveByTagAsync(string tag, CancellationToken cancellationToken = default);

    Task<T> GetOrAddAsync<T>(CacheKey key, Func<Task<T>> factory, CacheEntryOptions? options = null, CancellationToken cancellationToken = default);
}
