using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Oks.Domain.Base;
using Oks.Logging.Abstractions.Enums;
using Oks.Logging.Abstractions.Interfaces;
using Oks.Logging.Abstractions.Models;
using Oks.Persistence.Abstractions.Repositories;
using Microsoft.Extensions.Options;
using Oks.Persistence.EfCore.Options;
using Oks.Persistence.Abstractions.Caching;


namespace Oks.Persistence.EfCore.Repositories;

public class EfWriteRepository<TEntity, TKey>
    : EfReadRepository<TEntity, TKey>, IWriteRepository<TEntity, TKey>
    where TEntity : Entity<TKey>
{
    private readonly WriteTracker _writeTracker;
    private readonly IRepositoryCacheInvalidator? _cacheInvalidator;

    public EfWriteRepository(
        DbContext dbContext,
        WriteTracker writeTracker,
        IRepositoryCacheInvalidator? cacheInvalidator = null,
        IOksLogWriter? logWriter = null,
        IOptions<OksRepositoryLoggingOptions>? repoLogOptions = null)
        : base(dbContext, logWriter, repoLogOptions)
    {
        _writeTracker = writeTracker;
        _cacheInvalidator = cacheInvalidator;
    }

    public Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        return MeasureWriteAsync("Add", async () =>
        {
            await DbSet.AddAsync(entity, cancellationToken);
            _writeTracker.MarkWrite();
            _cacheInvalidator?.Invalidate<TEntity>();
        });
    }

    public void Update(TEntity entity)
    {
        MeasureWriteAsync("Update", () =>
        {
            DbSet.Update(entity);
            _writeTracker.MarkWrite();
            _cacheInvalidator?.Invalidate<TEntity>();
            return Task.CompletedTask;
        }).GetAwaiter().GetResult();
    }

    public void Remove(TEntity entity)
    {
        MeasureWriteAsync("Remove", () =>
        {
            DbSet.Remove(entity);
            _writeTracker.MarkWrite();
            _cacheInvalidator?.Invalidate<TEntity>();
            return Task.CompletedTask;
        }).GetAwaiter().GetResult();
    }

    private async Task MeasureWriteAsync(string operation, Func<Task> action)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            await action();
        }
        finally
        {
            sw.Stop();
            await LogRepositoryAsync(isWrite: true, operation, sw.ElapsedMilliseconds);
        }
    }
}
