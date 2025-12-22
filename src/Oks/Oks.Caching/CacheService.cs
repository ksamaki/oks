using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Oks.Caching.Abstractions;
using Oks.Caching.Internal;
using Oks.Caching.Tags;

namespace Oks.Caching;

public class CacheService : ICacheService
{
    private readonly IMemoryCache? _memoryCache;
    private readonly IDistributedCache? _distributedCache;
    private readonly ICacheSerializer _serializer;
    private readonly ICacheTagIndex _tagIndex;
    private readonly OksCachingOptions _options;
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

    public CacheService(
        IMemoryCache? memoryCache,
        IDistributedCache? distributedCache,
        ICacheSerializer serializer,
        ICacheTagIndex tagIndex,
        IOptions<OksCachingOptions>? options = null)
    {
        _memoryCache = memoryCache;
        _distributedCache = distributedCache;
        _serializer = serializer;
        _tagIndex = tagIndex;
        _options = options?.Value ?? new OksCachingOptions();
    }

    public async Task<T?> GetAsync<T>(CacheKey key, CancellationToken cancellationToken = default)
    {
        var lookup = await LookupAsync<T>(key, cancellationToken);
        return lookup.Value;
    }

    public async Task<T> GetOrAddAsync<T>(
        CacheKey key,
        Func<Task<T>> factory,
        CacheEntryOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var lookup = await LookupAsync<T>(key, cancellationToken);
        if (lookup.Value is not null && !lookup.ShouldRefresh)
            return lookup.Value;

        var semaphore = _locks.GetOrAdd(key.Value, _ => new SemaphoreSlim(1, 1));
        await semaphore.WaitAsync(cancellationToken);
        try
        {
            lookup = await LookupAsync<T>(key, cancellationToken);
            if (lookup.Value is not null && !lookup.ShouldRefresh)
                return lookup.Value;

            var produced = await factory();
            await SetAsync(key, produced, options, cancellationToken);
            return produced;
        }
        finally
        {
            semaphore.Release();
        }
    }

    public async Task SetAsync<T>(
        CacheKey key,
        T value,
        CacheEntryOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var effective = MergeOptions(options);
        var envelope = new CacheEnvelope<T>
        {
            Value = value,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            AbsoluteExpirationRelativeToNow = effective.AbsoluteExpirationRelativeToNow,
            SlidingExpiration = effective.SlidingExpiration,
            SoftExpiration = effective.SoftExpiration,
            Tags = effective.Tags
        };

        await StoreAsync(key, envelope, effective, cancellationToken);
    }

    public Task RemoveAsync(CacheKey key, CancellationToken cancellationToken = default)
    {
        _tagIndex.RemoveKey(key.Value);
        if (_options.Provider == CacheProvider.Memory)
        {
            _memoryCache?.Remove(key.Value);
            return Task.CompletedTask;
        }

        return _distributedCache?.RemoveAsync(key.Value, cancellationToken) ?? Task.CompletedTask;
    }

    public async Task RemoveByTagAsync(string tag, CancellationToken cancellationToken = default)
    {
        var keys = _tagIndex.KeysFor(tag);
        if (keys.Count == 0)
            return;

        foreach (var key in keys)
        {
            await RemoveAsync(new CacheKey(new[] { key }), cancellationToken);
        }

        _tagIndex.RemoveTag(tag);
    }

    private CacheEntryOptions MergeOptions(CacheEntryOptions? options)
    {
        if (options is null)
            return _options.DefaultEntryOptions;

        return new CacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = options.AbsoluteExpirationRelativeToNow ?? _options.DefaultEntryOptions.AbsoluteExpirationRelativeToNow,
            SlidingExpiration = options.SlidingExpiration ?? _options.DefaultEntryOptions.SlidingExpiration,
            SoftExpiration = options.SoftExpiration ?? _options.DefaultEntryOptions.SoftExpiration,
            Priority = options.Priority,
            Tags = options.Tags.Length > 0 ? options.Tags : _options.DefaultEntryOptions.Tags,
            Version = options.Version
        };
    }

    private async Task<CacheLookupResult<T>> LookupAsync<T>(CacheKey key, CancellationToken cancellationToken)
    {
        return _options.Provider == CacheProvider.Memory
            ? LookupMemory<T>(key)
            : await LookupDistributedAsync<T>(key, cancellationToken);
    }

    private CacheLookupResult<T> LookupMemory<T>(CacheKey key)
    {
        if (_memoryCache is null)
            return new CacheLookupResult<T>(default, shouldRefresh: false);

        if (!_memoryCache.TryGetValue(key.Value, out CacheEnvelope<T>? envelope))
            return new CacheLookupResult<T>(default, shouldRefresh: false);

        if (envelope is null)
            return new CacheLookupResult<T>(default, shouldRefresh: false);

        var shouldRefresh = envelope.SoftExpiration.HasValue &&
                            DateTimeOffset.UtcNow - envelope.CreatedAtUtc >= envelope.SoftExpiration.Value;

        return new CacheLookupResult<T>(envelope.Value, shouldRefresh);
    }

    private async Task<CacheLookupResult<T>> LookupDistributedAsync<T>(CacheKey key, CancellationToken cancellationToken)
    {
        if (_distributedCache is null)
            return new CacheLookupResult<T>(default, shouldRefresh: false);

        var bytes = await _distributedCache.GetAsync(key.Value, cancellationToken);
        if (bytes is null)
            return new CacheLookupResult<T>(default, shouldRefresh: false);

        var envelope = _serializer.Deserialize<CacheEnvelope<T>>(bytes);
        if (envelope is null)
            return new CacheLookupResult<T>(default, shouldRefresh: false);

        var shouldRefresh = envelope.SoftExpiration.HasValue &&
                            DateTimeOffset.UtcNow - envelope.CreatedAtUtc >= envelope.SoftExpiration.Value;

        return new CacheLookupResult<T>(envelope.Value, shouldRefresh);
    }

    private async Task StoreAsync<T>(CacheKey key, CacheEnvelope<T> envelope, CacheEntryOptions options, CancellationToken cancellationToken)
    {
        if (options.Tags.Length > 0)
        {
            _tagIndex.Map(key, options.Tags);
        }

        if (_options.Provider == CacheProvider.Memory)
        {
            if (_memoryCache is null)
                return;

            var entryOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = options.AbsoluteExpirationRelativeToNow,
                SlidingExpiration = options.SlidingExpiration,
                Priority = options.Priority
            };

            _memoryCache.Set(key.Value, envelope, entryOptions);
            return;
        }

        if (_distributedCache is null)
            return;

        var distributedOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = options.AbsoluteExpirationRelativeToNow,
            SlidingExpiration = options.SlidingExpiration
        };

        var bytes = _serializer.Serialize(envelope);
        await _distributedCache.SetAsync(key.Value, bytes, distributedOptions, cancellationToken);
    }
}
