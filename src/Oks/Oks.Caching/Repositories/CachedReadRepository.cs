using System.Linq.Expressions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Oks.Caching.Abstractions;
using Oks.Caching.Internal;
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
    private readonly RepositoryQueryCacheScope _queryScope;

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
        _queryScope = cachingOptions?.Value.RepositoryQueryCacheScope ?? RepositoryQueryCacheScope.ListOnly;
    }

    public IQueryable<TEntity> Query()
        => _inner.Query();

    public async Task<TEntity?> GetByIdAsync(
        TKey id,
        CancellationToken cancellationToken = default)
    {
        var policy = ResolvePolicy();
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
        var policy = ResolvePolicy();
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
        var policy = ResolvePolicy();
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

    private CachePolicy ResolvePolicy()
    {
        var entityCacheable = typeof(TEntity).GetCustomAttributes(typeof(CacheableAttribute), true)
            .Cast<CacheableAttribute>()
            .FirstOrDefault();
        var methodCacheable = CacheInvocationContextResolver.ResolveCacheable<CachedReadRepository<TEntity, TKey>>();
        var custom = CacheInvocationContextResolver.ResolveCustomCache<CachedReadRepository<TEntity, TKey>>();

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

    private CacheEntryOptions WithTags(IReadOnlyCollection<string> tags, CachePolicy policy)
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
