using Oks.Domain.Base;
using Oks.Location.Redis.Models;
using Oks.Persistence.Abstractions.Repositories;

namespace Oks.Location.Redis.Contracts;

[Obsolete("Use IGeoSpatialWriteRepository<TEntity, TKey> instead. ILocationWriteRepository is kept for backward compatibility.")]
public interface ILocationWriteRepository<TEntity, TKey> : IWriteRepository<TEntity, TKey>
    where TEntity : Entity<TKey>
{
    Task UpdateLocationAsync(TEntity entity, GeoPoint point, CancellationToken cancellationToken = default);
}
