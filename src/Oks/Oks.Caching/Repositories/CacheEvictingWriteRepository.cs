using System.Linq.Expressions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Oks.Caching.Abstractions;
using Oks.Caching.Internal;
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
    private readonly RepositoryQueryCacheScope _queryScope;

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
        _queryScope = cachingOptions?.Value.RepositoryQueryCacheScope ?? RepositoryQueryCacheScope.ListOnly;
    }

    public IQueryable<TEntity> Query()
        => _inner.Query();

    public async Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default)
    {
        var policy = ResolveReadPolicy();
        if (!policy.Enabled || _queryScope == RepositoryQueryCacheScope.ListOnly)
            return await _inner.GetByIdAsync(id, cancellationToken);

        var key = policy.KeyTemplate is { Length: > 0 }
            ? _keyBuilder.FromTemplate(policy.KeyTemplate, new { id })
            : _keyBuilder.ForRead<TEntity>("GetById", new { id });

        var options = WithTags(CacheTagHelper.ForEntity<TEntity, TKey>(id), policy);

        return await _cacheService.GetOrAddAsync(key,
            () => _inner.GetByIdAsync(id, cancellationToken),
            options,
            cancellationToken);
    }

    public async Task<TEntity?> GetAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        var policy = ResolveReadPolicy();
        if (!policy.Enabled || _queryScope == RepositoryQueryCacheScope.ListOnly)
            return await _inner.GetAsync(predicate, cancellationToken);

        var key = policy.KeyTemplate is { Length: > 0 }
            ? _keyBuilder.FromTemplate(policy.KeyTemplate, predicate.ToString())
            : _keyBuilder.ForRead<TEntity>("Get", predicate.ToString());

        var options = WithTags(CacheTagHelper.ForEntityName<TEntity>(), policy);

        return await _cacheService.GetOrAddAsync(key,
            () => _inner.GetAsync(predicate, cancellationToken),
            options,
            cancellationToken);
    }

    public async Task<List<TEntity>> GetListAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        var policy = ResolveReadPolicy();
        if (!policy.Enabled)
            return await _inner.GetListAsync(predicate, cancellationToken);

        var key = policy.KeyTemplate is { Length: > 0 }
            ? _keyBuilder.FromTemplate(policy.KeyTemplate, predicate?.ToString())
            : _keyBuilder.ForRead<TEntity>("GetList", predicate?.ToString());

        var options = WithTags(CacheTagHelper.ForEntityName<TEntity>(), policy);

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
        var entityCacheable = typeof(TEntity).GetCustomAttributes(typeof(CacheableAttribute), true).Length > 0;
        var custom = CacheInvocationContextResolver.ResolveCustomCache<CacheEvictingWriteRepository<TEntity, TKey>>();

        if (!entityCacheable && custom is null && !CacheInvocationContextResolver.ResolveEvictAttributes<CacheEvictingWriteRepository<TEntity, TKey>>().Any())
            return;

        var tags = new HashSet<string>(CacheTagHelper.ForEntity<TEntity, TKey>(entity));

        if (custom is not null)
        {
            foreach (var tag in custom.Tags)
            {
                tags.Add(tag);
            }

            if (custom.EvictAllEntityCache)
            {
                tags.Add(typeof(TEntity).Name);
                tags.Add($"Query:{typeof(TEntity).Name}");
            }
        }

        foreach (var attribute in CacheInvocationContextResolver.ResolveEvictAttributes<CacheEvictingWriteRepository<TEntity, TKey>>())
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

    private CachePolicy ResolveReadPolicy()
    {
        var entityCacheable = typeof(TEntity).GetCustomAttributes(typeof(CacheableAttribute), true)
            .Cast<CacheableAttribute>()
            .FirstOrDefault();
        var methodCacheable = CacheInvocationContextResolver.ResolveCacheable<CacheEvictingWriteRepository<TEntity, TKey>>();
        var custom = CacheInvocationContextResolver.ResolveCustomCache<CacheEvictingWriteRepository<TEntity, TKey>>();

        var hasCacheable = entityCacheable is not null || methodCacheable is not null;
        var hasCustomCache = custom is { Evict: false };

        var enabled = hasCacheable || hasCustomCache;
        var source = custom is { Evict: false }
            ? custom
            : (object?)methodCacheable ?? entityCacheable;

        return new CachePolicy(
            enabled,
            source switch
            {
                CustomCacheAttribute customCache => customCache.KeyTemplate,
                CacheableAttribute cacheable => cacheable.KeyTemplate,
                _ => string.Empty
            },
            source switch
            {
                CustomCacheAttribute customCache => customCache.DurationSeconds,
                CacheableAttribute cacheable => cacheable.DurationSeconds,
                _ => 0
            },
            source switch
            {
                CustomCacheAttribute customCache => customCache.Tags,
                CacheableAttribute cacheable => cacheable.Tags,
                _ => Array.Empty<string>()
            });
    }

    private CacheEntryOptions WithTags(
        IReadOnlyCollection<string> tags,
        CachePolicy policy)
    {
        var mergedTags = policy.Tags.Length > 0
            ? tags.Concat(policy.Tags).Distinct().ToArray()
            : tags.ToArray();

        var absolute = policy.DurationSeconds > 0
            ? TimeSpan.FromSeconds(policy.DurationSeconds)
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

    private sealed record CachePolicy(bool Enabled, string KeyTemplate, int DurationSeconds, string[] Tags);
}
