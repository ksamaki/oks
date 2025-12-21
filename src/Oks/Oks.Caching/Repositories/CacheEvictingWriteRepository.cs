using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Oks.Caching.Abstractions;
using Oks.Caching.Tags;
using Oks.Domain.Base;
using Oks.Logging.Abstractions.Interfaces;
using Oks.Persistence.Abstractions.Repositories;
using Oks.Persistence.EfCore;
using Oks.Persistence.EfCore.Options;
using Oks.Persistence.EfCore.Repositories;

namespace Oks.Caching.Repositories;

public class CacheEvictingWriteRepository<TEntity, TKey>
    : IWriteRepository<TEntity, TKey>
    where TEntity : Entity<TKey>
{
    private readonly EfWriteRepository<TEntity, TKey> _inner;
    private readonly ICacheService _cacheService;
    private readonly ICacheKeyBuilder _keyBuilder;
    private readonly CacheEntryOptions _defaults;

    public CacheEvictingWriteRepository(
        DbContext dbContext,
        WriteTracker writeTracker,
        ICacheService cacheService,
        ICacheKeyBuilder keyBuilder,
        IOksLogWriter? logWriter = null,
        IOptions<OksRepositoryLoggingOptions>? repoLogOptions = null,
        IOptions<OksCachingOptions>? cachingOptions = null)
    {
        _inner = new EfWriteRepository<TEntity, TKey>(dbContext, writeTracker, logWriter, repoLogOptions);
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
        foreach (var tag in CacheTagHelper.ForEntity(entity))
        {
            await _cacheService.RemoveByTagAsync(tag, cancellationToken);
        }
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
