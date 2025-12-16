using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Oks.Domain.Attributes;
using Oks.Domain.Base;
using Oks.Logging.Abstractions.Interfaces;
using Oks.Persistence.Abstractions.Caching;
using Oks.Persistence.EfCore.Options;

namespace Oks.Persistence.EfCore.Repositories;

public class CachedEfReadRepository<TEntity, TKey>
    : EfReadRepository<TEntity, TKey>
    where TEntity : Entity<TKey>
{
    private readonly IMemoryCache _cache;
    private readonly IRepositoryCacheTokenProvider _cacheTokenProvider;
    private readonly OksRepositoryCacheOptions _cacheOptions;
    private readonly EnableRepositoryCacheAttribute? _cacheAttribute;

    public CachedEfReadRepository(
        DbContext dbContext,
        IMemoryCache cache,
        IRepositoryCacheTokenProvider cacheTokenProvider,
        IOksLogWriter? logWriter = null,
        IOptions<OksRepositoryLoggingOptions>? repoLogOptions = null,
        IOptions<OksRepositoryCacheOptions>? cacheOptions = null)
        : base(dbContext, logWriter, repoLogOptions)
    {
        _cache = cache;
        _cacheTokenProvider = cacheTokenProvider;
        _cacheOptions = cacheOptions?.Value ?? new OksRepositoryCacheOptions();
        _cacheAttribute = typeof(TEntity).GetCustomAttributes(typeof(EnableRepositoryCacheAttribute), inherit: false)
            .OfType<EnableRepositoryCacheAttribute>()
            .FirstOrDefault();
    }

    public override IQueryable<TEntity> Query()
        => base.Query();

    public override async Task<TEntity?> GetByIdAsync(
        TKey id,
        CancellationToken cancellationToken = default)
    {
        if (!IsCacheEnabled())
        {
            return await base.GetByIdAsync(id, cancellationToken);
        }

        var cacheKey = BuildCacheKey("id", id!);
        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            ApplyCacheOptions(entry);
            return await base.GetByIdAsync(id, cancellationToken);
        });
    }

    public override async Task<List<TEntity>> GetListAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        if (!IsCacheEnabled())
        {
            return await base.GetListAsync(predicate, cancellationToken);
        }

        var cacheKey = BuildCacheKey("list", predicate?.ToString() ?? "all");
        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            ApplyCacheOptions(entry);
            return await base.GetListAsync(predicate, cancellationToken);
        });
    }

    private bool IsCacheEnabled()
    {
        if (!_cacheOptions.Enabled)
            return false;

        if (!_cacheOptions.RespectEntityAttribute)
            return true;

        return _cacheAttribute is not null;
    }

    private string BuildCacheKey(string prefix, object discriminator)
        => $"oks:repo-cache:{typeof(TEntity).FullName}:{prefix}:{discriminator}";

    private void ApplyCacheOptions(ICacheEntry entry)
    {
        var absoluteExpiration = _cacheAttribute?.AbsoluteExpiration ?? _cacheOptions.DefaultAbsoluteExpiration;
        var slidingExpiration = _cacheAttribute?.SlidingExpiration ?? _cacheOptions.DefaultSlidingExpiration;

        entry.AbsoluteExpirationRelativeToNow = absoluteExpiration;
        entry.SlidingExpiration = slidingExpiration;
        entry.AddExpirationToken(_cacheTokenProvider.GetChangeToken<TEntity>());
    }
}
