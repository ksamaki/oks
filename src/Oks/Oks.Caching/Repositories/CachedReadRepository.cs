using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Oks.Caching.Abstractions;
using Oks.Caching.Tags;
using Oks.Domain.Base;
using Oks.Persistence.Abstractions.Repositories;

namespace Oks.Caching.Repositories;

public class CachedReadRepository<TEntity, TKey>
    : IReadRepository<TEntity, TKey>
    where TEntity : Entity<TKey>
{
    private readonly IReadRepository<TEntity, TKey> _inner;
    private readonly ICacheService _cacheService;
    private readonly ICacheKeyBuilder _keyBuilder;
    private readonly CacheEntryOptions _defaults;

    public CachedReadRepository(
        [FromKeyedServices("base")] IReadRepository<TEntity, TKey> inner,
        ICacheService cacheService,
        ICacheKeyBuilder keyBuilder,
        IOptions<OksCachingOptions>? cachingOptions = null)
    {
        _inner = inner;
        _cacheService = cacheService;
        _keyBuilder = keyBuilder;
        _defaults = cachingOptions?.Value.DefaultEntryOptions ?? new CacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
            SoftExpiration = TimeSpan.FromSeconds(30)
        };
    }

    public IQueryable<TEntity> Query()
        => _inner.Query();

    public async Task<TEntity?> GetByIdAsync(
        TKey id,
        CancellationToken cancellationToken = default)
    {
        var cacheable = ResolveCacheableAttribute();
        var key = cacheable?.KeyTemplate is { Length: > 0 }
            ? _keyBuilder.FromTemplate(cacheable.KeyTemplate, new { id })
            : _keyBuilder.ForRead<TEntity>("GetById", new { id });

        var options = WithTags(CacheTagHelper.ForEntity<TEntity, TKey>(id), cacheable);

        return await _cacheService.GetOrAddAsync(key,
            () => _inner.GetByIdAsync(id, cancellationToken),
            options,
            cancellationToken);
    }

    public async Task<List<TEntity>> GetListAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        var cacheable = ResolveCacheableAttribute();
        var key = cacheable?.KeyTemplate is { Length: > 0 }
            ? _keyBuilder.FromTemplate(cacheable.KeyTemplate, predicate?.ToString())
            : _keyBuilder.ForRead<TEntity>("GetList", predicate?.ToString());

        var options = WithTags(CacheTagHelper.ForEntityName<TEntity>(), cacheable);

        return await _cacheService.GetOrAddAsync(key,
            () => _inner.GetListAsync(predicate, cancellationToken),
            options,
            cancellationToken);
    }

    private CacheableAttribute? ResolveCacheableAttribute()
    {
        var trace = new StackTrace();
        foreach (var frame in trace.GetFrames() ?? Array.Empty<StackFrame>())
        {
            var method = frame.GetMethod();
            if (method is null)
                continue;

            if (method.DeclaringType?.Assembly == typeof(CachedReadRepository<,>).Assembly)
                continue;

            var attribute = method.GetCustomAttribute<CacheableAttribute>(inherit: true);
            if (attribute is not null)
                return attribute;
        }

        return null;
    }

    private CacheEntryOptions WithTags(
        IReadOnlyCollection<string> tags,
        CacheableAttribute? cacheable)
    {
        var mergedTags = cacheable?.Tags is { Length: > 0 }
            ? tags.Concat(cacheable.Tags).Distinct().ToArray()
            : tags.ToArray();

        // FIX: cacheable.DurationSeconds is an int. Previously code used .Value which caused compilation errors.
        var absolute = cacheable != null
            ? TimeSpan.FromSeconds(cacheable.DurationSeconds)
            : _defaults.AbsoluteExpirationRelativeToNow;

        return new CacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = absolute,
            SlidingExpiration = _defaults.SlidingExpiration,
            SoftExpiration = _defaults.SoftExpiration,
            Priority = _defaults.Priority,
            Tags = mergedTags
        };
    }
}
