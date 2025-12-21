using System.Linq.Expressions;
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
        var key = _keyBuilder.ForRead<TEntity>("GetById", new { id });
        var options = WithTags(CacheTagHelper.ForEntity<TEntity, TKey>(id));

        return await _cacheService.GetOrAddAsync(key,
            () => _inner.GetByIdAsync(id, cancellationToken),
            options,
            cancellationToken);
    }

    public async Task<List<TEntity>> GetListAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        var key = _keyBuilder.ForRead<TEntity>("GetList", predicate?.ToString());
        var options = WithTags(CacheTagHelper.ForEntityName<TEntity>());

        return await _cacheService.GetOrAddAsync(key,
            () => _inner.GetListAsync(predicate, cancellationToken),
            options,
            cancellationToken);
    }

    private CacheEntryOptions WithTags(IReadOnlyCollection<string> tags)
    {
        return new CacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = _defaults.AbsoluteExpirationRelativeToNow,
            SlidingExpiration = _defaults.SlidingExpiration,
            SoftExpiration = _defaults.SoftExpiration,
            Priority = _defaults.Priority,
            Tags = tags.ToArray()
        };
    }
}
