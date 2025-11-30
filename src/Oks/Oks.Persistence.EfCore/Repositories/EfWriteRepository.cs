using Microsoft.EntityFrameworkCore;
using Oks.Domain.Base;
using Oks.Persistence.Abstractions.Repositories;

namespace Oks.Persistence.EfCore.Repositories;

public class EfWriteRepository<TEntity, TKey>
    : EfReadRepository<TEntity, TKey>, IWriteRepository<TEntity, TKey>
    where TEntity : Entity<TKey>
{
    private readonly WriteTracker _writeTracker;

    public EfWriteRepository(DbContext dbContext, WriteTracker writeTracker)
        : base(dbContext)
    {
        _writeTracker = writeTracker;
    }

    public Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        DbSet.Add(entity);
        _writeTracker.MarkWrite();
        return Task.CompletedTask;
    }

    public void Update(TEntity entity)
    {
        DbSet.Update(entity);
        _writeTracker.MarkWrite();
    }

    public void Remove(TEntity entity)
    {
        DbSet.Remove(entity);
        _writeTracker.MarkWrite();
    }
}
