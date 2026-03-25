using System.Diagnostics;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Oks.Domain.Base;
using Oks.Logging.Abstractions.Enums;
using Oks.Logging.Abstractions.Interfaces;
using Oks.Logging.Abstractions.Models;
using Oks.Persistence.Abstractions.Repositories;
using Microsoft.Extensions.Options;
using Oks.Persistence.EfCore.Options;


namespace Oks.Persistence.EfCore.Repositories;

public class EfWriteRepository<TEntity, TKey>
    : EfReadRepository<TEntity, TKey>, IWriteRepository<TEntity, TKey>
    where TEntity : Entity<TKey>
{
    public EfWriteRepository(
        DbContext dbContext,
        IOksLogWriter? logWriter = null,
        IOptions<OksRepositoryLoggingOptions>? repoLogOptions = null)
        : base(dbContext, logWriter, repoLogOptions)
    {
    }

    public Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        return MeasureWriteAsync("Add", async () =>
        {
            await DbSet.AddAsync(entity, cancellationToken);
        });
    }

    public void Update(TEntity entity)
    {
        MeasureWriteAsync("Update", () =>
        {
            DbSet.Update(entity);
            return Task.CompletedTask;
        }).GetAwaiter().GetResult();
    }

    public void Remove(TEntity entity)
    {
        MeasureWriteAsync("Remove", () =>
        {
            DbSet.Remove(entity);
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
