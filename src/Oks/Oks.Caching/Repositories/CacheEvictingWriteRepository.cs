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

    public IQueryable<TEntity> Query() => _inner.Query();

    public async Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default)
    {
        var policy = ResolveReadPolicy();
        if (!policy.Enabled || _queryScope == RepositoryQueryCacheScope.ListOnly)
            return await _inner.GetByIdAsync(id, cancellationToken);

        var key = _keyBuilder.ForRead<TEntity>("GetById", new { id });
        return await _cacheService.GetOrAddAsync(key, () => _inner.GetByIdAsync(id, cancellationToken), WithTags(CacheTagHelper.ForEntity<TEntity, TKey>(id), policy), cancellationToken);
    }

    public async Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        var policy = ResolveReadPolicy();
        if (!policy.Enabled || _queryScope == RepositoryQueryCacheScope.ListOnly)
            return await _inner.GetAsync(predicate, cancellationToken);

        var key = _keyBuilder.ForRead<TEntity>("Get", predicate.ToString());
        return await _cacheService.GetOrAddAsync(key, () => _inner.GetAsync(predicate, cancellationToken), WithTags(CacheTagHelper.ForEntityName<TEntity>(), policy), cancellationToken);
    }

    public async Task<List<TEntity>> GetListAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default)
    {
        var policy = ResolveReadPolicy();
        if (!policy.Enabled)
            return await _inner.GetListAsync(predicate, cancellationToken);

        var key = _keyBuilder.ForRead<TEntity>("GetList", predicate?.ToString());
        return await _cacheService.GetOrAddAsync(key, () => _inner.GetListAsync(predicate, cancellationToken), WithTags(CacheTagHelper.ForEntityName<TEntity>(), policy), cancellationToken);
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
        var entityCacheable = typeof(TEntity).GetCustomAttributes(typeof(OksEntityCacheAttribute), true).Length > 0;
        var invalidateAttributes = CacheInvocationContextResolver.ResolveInvalidationAttributes<CacheEvictingWriteRepository<TEntity, TKey>>();

        if (!entityCacheable && !invalidateAttributes.Any())
            return;

        var tags = new HashSet<string>(CacheTagHelper.ForEntity<TEntity, TKey>(entity));
        foreach (var attribute in invalidateAttributes)
        {
            foreach (var tag in attribute.Tags)
                tags.Add(tag);
        }

        foreach (var tag in tags)
            await _cacheService.RemoveByTagAsync(tag, cancellationToken);
    }

    private CachePolicy ResolveReadPolicy()
    {
        var entityCache = typeof(TEntity).GetCustomAttributes(typeof(OksEntityCacheAttribute), true)
            .Cast<OksEntityCacheAttribute>()
            .FirstOrDefault();

        if (entityCache is null)
            return new CachePolicy(false, null, Array.Empty<string>());

        return new CachePolicy(true, entityCache.TtlSeconds, entityCache.Tags);
    }

    private CacheEntryOptions WithTags(IReadOnlyCollection<string> tags, CachePolicy policy)
    {
        var mergedTags = policy.Tags.Length > 0 ? tags.Concat(policy.Tags).Distinct().ToArray() : tags.ToArray();
        var absolute = policy.TtlSeconds.HasValue ? TimeSpan.FromSeconds(policy.TtlSeconds.Value) : _defaults.AbsoluteExpirationRelativeToNow;

        return new CacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = absolute,
            SlidingExpiration = _defaults.SlidingExpiration,
            SoftExpiration = _defaults.SoftExpiration,
            Priority = _defaults.Priority,
            Tags = mergedTags
        };
    }

    private sealed record CachePolicy(bool Enabled, int? TtlSeconds, string[] Tags);
}
