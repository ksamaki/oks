using Oks.Domain.Base;

namespace Oks.Persistence.Abstractions.Repositories;

public interface IWriteRepository<TEntity, TKey>
    : IReadRepository<TEntity, TKey>
    where TEntity : Entity<TKey>
{
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    void Update(TEntity entity);

    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

    void Remove(TEntity entity);

    Task RemoveAsync(TEntity entity, CancellationToken cancellationToken = default);
}
