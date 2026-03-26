using Oks.Domain.Base;
using Oks.Location.Redis.Models;
using Oks.Persistence.Abstractions.Repositories;

namespace Oks.Location.Redis.Contracts;

public interface IGeoSpatialWriteRepository<TEntity, TKey> : IWriteRepository<TEntity, TKey>
    where TEntity : Entity<TKey>
{
    Task UpdatePositionAsync(TEntity entity, GeoCoordinate point, CancellationToken cancellationToken = default);
}
