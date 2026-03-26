using System.Linq.Expressions;
using Oks.Domain.Base;

namespace Oks.Persistence.Abstractions.Repositories;

public interface IReadRepository<TEntity, TKey>
    where TEntity : Entity<TKey>
{
    IQueryable<TEntity> Query();

    Task<TEntity?> GetAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default);

    Task<TEntity?> GetByIdAsync(
        TKey id,
        CancellationToken cancellationToken = default);

    Task<List<TEntity>> GetListAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default);
}
