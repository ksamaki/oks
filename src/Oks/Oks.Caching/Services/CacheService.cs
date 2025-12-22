namespace Oks.Caching.Services;

using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Oks.Caching.Abstractions.Interfaces;
using Oks.Caching.Abstractions.Models;
using Oks.Caching.Options;

public sealed class CacheService : ICacheService
{
    private readonly IMemoryCache? _memoryCache;
    private readonly IDistributedCache? _distributedCache;
    private readonly ICacheSerializer _serializer;
    private readonly ILogger<CacheService>? _logger;
    private readonly OksCachingOptions _options;
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

    public CacheService(
        IMemoryCache? memoryCache,
        IDistributedCache? distributedCache,
        ICacheSerializer serializer,
        IOptions<OksCachingOptions> options,
        ILogger<CacheService>? logger = null)
    {
        _memoryCache = memoryCache;
        _distributedCache = distributedCache;
        _serializer = serializer;
        _logger = logger;
        _options = options.Value;
    }

    public async Task<T?> GetAsync<T>(CacheKey key, CancellationToken cancellationToken = default)
    {
        if (_options.Provider == OksCachingProvider.Memory)
        {
            if (_memoryCache is not null && _memoryCache.TryGetValue(key.Value, out var cached))
            {
                return (T?)cached;
            }

            return default;
        }

        if (_distributedCache is null)
        {
            return default;
        }

        var payload = await _distributedCache.GetAsync(key.Value, cancellationToken);
        return _serializer.Deserialize<T>(payload);
    }

    public async Task<T> GetOrAddAsync<T>(CacheKey key, Func<Task<T>> factory, CacheEntryOptions? options = null, CancellationToken cancellationToken = default)
    {
        var cached = await GetAsync<T>(key, cancellationToken);
        if (cached is not null)
        {
            return cached;
        }

        var locker = _locks.GetOrAdd(key.Value, _ => new SemaphoreSlim(1, 1));

        await locker.WaitAsync(cancellationToken);
        try
        {
            cached = await GetAsync<T>(key, cancellationToken);
            if (cached is not null)
            {
                return cached;
            }

            var produced = await factory();
            await SetAsync(key, produced, options, cancellationToken);
            return produced;
        }
        finally
        {
            locker.Release();
        }
    }

    public async Task RemoveAsync(CacheKey key, CancellationToken cancellationToken = default)
    {
        if (_options.Provider == OksCachingProvider.Memory)
        {
            _memoryCache?.Remove(key.Value);
            return;
        }

        if (_distributedCache is null)
        {
            return;
        }

        await _distributedCache.RemoveAsync(key.Value, cancellationToken);
    }

    public async Task RemoveByTagAsync(string tag, CancellationToken cancellationToken = default)
    {
        var tagIndexKey = GetTagIndexKey(tag);
        var taggedKeys = await GetTagIndexAsync(tagIndexKey, cancellationToken);

        foreach (var key in taggedKeys)
        {
            await RemoveAsync(new CacheKey(key), cancellationToken);
        }

        await RemoveAsync(new CacheKey(tagIndexKey), cancellationToken);
    }

    public async Task SetAsync<T>(CacheKey key, T value, CacheEntryOptions? options = null, CancellationToken cancellationToken = default)
    {
        var effectiveOptions = MergeOptions(options);
        if (_options.Provider == OksCachingProvider.Memory)
        {
            if (_memoryCache is null)
            {
                return;
            }

            var entryOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = effectiveOptions.AbsoluteExpirationRelativeToNow,
                SlidingExpiration = effectiveOptions.SlidingExpiration
            };

            _memoryCache.Set(key.Value, value, entryOptions);
            await TrackTagsAsync(key, effectiveOptions, cancellationToken);
            return;
        }

        if (_distributedCache is null)
        {
            return;
        }

        var payload = _serializer.Serialize(value);
        var distributedOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = effectiveOptions.AbsoluteExpirationRelativeToNow,
            SlidingExpiration = effectiveOptions.SlidingExpiration
        };

        await _distributedCache.SetAsync(key.Value, payload, distributedOptions, cancellationToken);
        await TrackTagsAsync(key, effectiveOptions, cancellationToken);
    }

    private CacheEntryOptions MergeOptions(CacheEntryOptions? options)
    {
        if (options is null)
        {
            return _options.DefaultEntryOptions;
        }

        var merged = new CacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = options.AbsoluteExpirationRelativeToNow ?? _options.DefaultEntryOptions.AbsoluteExpirationRelativeToNow,
            SlidingExpiration = options.SlidingExpiration ?? _options.DefaultEntryOptions.SlidingExpiration,
            SoftTtl = options.SoftTtl ?? _options.DefaultEntryOptions.SoftTtl,
            Version = options.Version ?? _options.DefaultEntryOptions.Version,
            Tags = options.Tags.Length > 0 ? options.Tags : _options.DefaultEntryOptions.Tags
        };

        return merged;
    }

    private async Task TrackTagsAsync(CacheKey key, CacheEntryOptions options, CancellationToken cancellationToken)
    {
        foreach (var tag in options.Tags)
        {
            var tagIndexKey = GetTagIndexKey(tag);
            var index = await GetTagIndexAsync(tagIndexKey, cancellationToken);
            index.Add(key.Value);
            await PersistTagIndexAsync(tagIndexKey, index, cancellationToken);
        }
    }

    private async Task<HashSet<string>> GetTagIndexAsync(string tagIndexKey, CancellationToken cancellationToken)
    {
        if (_options.Provider == OksCachingProvider.Memory)
        {
            if (_memoryCache is null)
            {
                return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }

            if (_memoryCache.TryGetValue(tagIndexKey, out var cached) && cached is HashSet<string> existing)
            {
                return existing;
            }

            var index = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            _memoryCache.Set(tagIndexKey, index);
            return index;
        }

        if (_distributedCache is null)
        {
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        var payload = await _distributedCache.GetAsync(tagIndexKey, cancellationToken);
        var deserialized = _serializer.Deserialize<HashSet<string>>(payload);
        return deserialized ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    }

    private async Task PersistTagIndexAsync(string tagIndexKey, HashSet<string> index, CancellationToken cancellationToken)
    {
        if (_options.Provider == OksCachingProvider.Memory)
        {
            _memoryCache?.Set(tagIndexKey, index);
            return;
        }

        if (_distributedCache is null)
        {
            return;
        }

        var payload = _serializer.Serialize(index);
        await _distributedCache.SetAsync(tagIndexKey, payload, cancellationToken);
    }

    private static string GetTagIndexKey(string tag) => $"oks:tag:{tag}";
}
