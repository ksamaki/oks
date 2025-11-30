using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Oks.Domain.Base;
using Oks.Persistence.Abstractions.Repositories;

namespace Oks.Persistence.EfCore.Repositories;

public class EfReadRepository<TEntity, TKey>
    : IReadRepository<TEntity, TKey>
    where TEntity : Entity<TKey>
{
    protected readonly DbContext DbContext;
    protected readonly DbSet<TEntity> DbSet;

    public EfReadRepository(DbContext dbContext)
    {
        DbContext = dbContext;
        DbSet = dbContext.Set<TEntity>();
    }

    public IQueryable<TEntity> Query()
        => DbSet.AsQueryable();

    public async Task<TEntity?> GetByIdAsync(
        TKey id,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.FindAsync(new object[] { id! }, cancellationToken);
    }

    public async Task<List<TEntity>> GetListAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> query = DbSet;

        if (predicate is not null)
        {
            query = query.Where(predicate);
        }

        return await query.ToListAsync(cancellationToken);
    }
}
