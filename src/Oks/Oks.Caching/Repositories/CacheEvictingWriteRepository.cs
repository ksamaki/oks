using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Oks.Caching.Abstractions;
using Oks.Caching.Tags;
using Oks.Domain.Base;
using Oks.Persistence.Abstractions.Repositories;

namespace Oks.Caching.Repositories;

public class CacheEvictingWriteRepository<TEntity, TKey>
    : IWriteRepository<TEntity, TKey>
    where TEntity : Entity<TKey>
{
    private readonly IWriteRepository<TEntity, TKey> _inner;
    private readonly ICacheService _cacheService;
    private readonly ICacheKeyBuilder _keyBuilder;
    private readonly CacheEntryOptions _defaults;

    public CacheEvictingWriteRepository(
        [FromKeyedServices("base")] IWriteRepository<TEntity, TKey> inner,
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

    public async Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default)
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

    public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await _inner.AddAsync(entity, cancellationToken);
        await EvictAsync(entity, cancellationToken);
    }

    public void Update(TEntity entity)
    {
        _inner.Update(entity);
        EvictAsync(entity).GetAwaiter().GetResult();
    }

    public void Remove(TEntity entity)
    {
        _inner.Remove(entity);
        EvictAsync(entity).GetAwaiter().GetResult();
    }

    private async Task EvictAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        var tags = new HashSet<string>(CacheTagHelper.ForEntity(entity));

        foreach (var attribute in ResolveCacheEvictAttributes())
        {
            foreach (var tag in attribute.Tags)
            {
                tags.Add(tag);
            }

            if (attribute.EvictAllEntityCache)
            {
                tags.Add(typeof(TEntity).Name);
                tags.Add($"Query:{typeof(TEntity).Name}");
            }
        }

        foreach (var tag in tags)
        {
            await _cacheService.RemoveByTagAsync(tag, cancellationToken);
        }
    }

    private CacheableAttribute? ResolveCacheableAttribute()
    {
        var trace = new StackTrace();
        foreach (var frame in trace.GetFrames() ?? Array.Empty<StackFrame>())
        {
            var method = frame.GetMethod();
            if (method is null)
                continue;

            if (method.DeclaringType?.Assembly == typeof(CacheEvictingWriteRepository<,>).Assembly)
                continue;

            var attribute = method.GetCustomAttribute<CacheableAttribute>(inherit: true);
            if (attribute is not null)
                return attribute;
        }

        return null;
    }

    private IEnumerable<CacheEvictAttribute> ResolveCacheEvictAttributes()
    {
        var trace = new StackTrace();
        foreach (var frame in trace.GetFrames() ?? Array.Empty<StackFrame>())
        {
            var method = frame.GetMethod();
            if (method is null)
                continue;

            if (method.DeclaringType?.Assembly == typeof(CacheEvictingWriteRepository<,>).Assembly)
                continue;

            foreach (var attribute in method.GetCustomAttributes<CacheEvictAttribute>(inherit: true))
            {
                yield return attribute;
            }
        }
    }

    private CacheEntryOptions WithTags(
        IReadOnlyCollection<string> tags,
        CacheableAttribute? cacheable)
    {
        var mergedTags = cacheable?.Tags is { Length: > 0 }
            ? tags.Concat(cacheable.Tags).Distinct().ToArray()
            : tags.ToArray();

        var absolute = cacheable?.DurationSeconds is not null
            ? TimeSpan.FromSeconds(cacheable.DurationSeconds.Value)
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
