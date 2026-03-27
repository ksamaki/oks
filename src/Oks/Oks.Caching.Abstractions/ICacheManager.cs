namespace Oks.Caching.Abstractions;

public interface ICacheManager
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

    Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, CacheEntryOptions? options = null, CancellationToken cancellationToken = default);

    Task SetAsync<T>(string key, T value, CacheEntryOptions? options = null, CancellationToken cancellationToken = default);

    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    Task RemoveByTagAsync(string tag, CancellationToken cancellationToken = default);
}
