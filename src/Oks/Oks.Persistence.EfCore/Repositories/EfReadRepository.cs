using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Oks.Domain.Base;
using Oks.Logging.Abstractions.Enums;
using Oks.Logging.Abstractions.Extensions;
using Oks.Logging.Abstractions.Interfaces;
using Oks.Logging.Abstractions.Models;
using Oks.Persistence.Abstractions.Repositories;
using Oks.Persistence.EfCore.Options;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Text.Json;

namespace Oks.Persistence.EfCore.Repositories;

public class EfReadRepository<TEntity, TKey>
    : IReadRepository<TEntity, TKey>
    where TEntity : Entity<TKey>
{
    protected readonly DbContext DbContext;
    protected readonly DbSet<TEntity> DbSet;

    private readonly IOksLogWriter? _logWriter;
    private readonly OksRepositoryLoggingOptions _repoLogOptions;

    public EfReadRepository(
        DbContext dbContext,
        IOksLogWriter? logWriter = null,
        IOptions<OksRepositoryLoggingOptions>? repoLogOptions = null)
    {
        DbContext = dbContext;
        DbSet = dbContext.Set<TEntity>();

        _logWriter = logWriter;
        _repoLogOptions = repoLogOptions?.Value ?? new OksRepositoryLoggingOptions
        {
            Enabled = false
        };
    }

    public IQueryable<TEntity> Query()
        => DbSet.AsNoTracking();

    public async Task<TEntity?> GetAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await MeasureReadAsync(
            "Get",
            async () => await DbSet.AsNoTracking()
                .Where(predicate)
                .FirstOrDefaultAsync(cancellationToken));
    }

    public async Task<TEntity?> GetByIdAsync(
        TKey id,
        CancellationToken cancellationToken = default)
    {
        var parameter = Expression.Parameter(typeof(TEntity), "x");
        var idProperty = Expression.Property(parameter, nameof(Entity<TKey>.Id));
        var constant = Expression.Constant(id, typeof(TKey));
        var body = Expression.Equal(idProperty, constant);
        var predicate = Expression.Lambda<Func<TEntity, bool>>(body, parameter);

        return await MeasureReadAsync(
            "GetById",
            async () => await DbSet.AsNoTracking()
                .FirstOrDefaultAsync(predicate, cancellationToken));
    }

    public async Task<List<TEntity>> GetListAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        return await MeasureReadAsync(
            "GetList",
            async () =>
            {
                IQueryable<TEntity> query = DbSet.AsNoTracking();

                if (predicate is not null)
                {
                    query = query.Where(predicate);
                }

                return await query.ToListAsync(cancellationToken);
            });
    }

    private async Task<T> MeasureReadAsync<T>(
        string operation,
        Func<Task<T>> action)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            return await action();
        }
        finally
        {
            sw.Stop();
            await LogRepositoryAsync(isWrite: false, operation, sw.ElapsedMilliseconds);
        }
    }

    protected async Task LogRepositoryAsync(bool isWrite, string operation, long elapsedMs)
    {
        // Logging hiç kurulmaması durumu
        if (_logWriter is null)
            return;

        // Repository logging özel olarak açılmamışsa
        if (!_repoLogOptions.Enabled)
            return;

        try
        {
            var entry = new OksLogEntry
            {
                Category = isWrite ? OksLogCategory.RepositoryWrite : OksLogCategory.RepositoryRead,
                Level = OksLogLevel.Info,
                Message = $"Repository {(isWrite ? "WRITE" : "READ")} on {typeof(TEntity).Name} ({operation}) took {elapsedMs} ms.",
                CreatedAtUtc = DateTime.UtcNow,
                ElapsedMilliseconds = elapsedMs,
                ExtraDataJson = JsonSerializer.Serialize(new
                {
                    EntityName = typeof(TEntity).Name,
                    Operation = operation,
                    ElapsedMs = elapsedMs
                })
            };

            await _logWriter.SafeWriteAsync(entry);
        }
        catch
        {
            // Repository davranışını bozmamak için log hatasını swallow ediyoruz.
        }
    }
}
