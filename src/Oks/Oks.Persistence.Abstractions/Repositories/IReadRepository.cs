using System.Linq.Expressions;
using Oks.Domain.Base;

namespace Oks.Persistence.Abstractions.Repositories;

public interface IReadRepository<TEntity, TKey>
    where TEntity : Entity<TKey>
{
    IQueryable<TEntity> Query();

    IQueryable<TEntity> Query(Expression<Func<TEntity, bool>> predicate);

    Task<TEntity?> GetByIdAsync(
        TKey id,
        CancellationToken cancellationToken = default);

    Task<List<TEntity>> GetListAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default);
}
