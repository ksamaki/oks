# Location Redis - Usage

[Location Redis - Description](LocationRedis_Description.md) | [Ana sayfa](../README.md)

Aşağıdaki örnek, `Oks.Location.Redis` kontratlarının domain bağımsız GeoSpatial kullanımını gösterir.

## 1) Proje referansları (`.csproj`)
```xml
<ItemGroup>
  <ProjectReference Include="..\src\Oks\Oks.Location.Redis\Oks.Location.Redis.csproj" />
  <ProjectReference Include="..\src\Oks\Oks.Persistence.Abstractions\Oks.Persistence.Abstractions.csproj" />
</ItemGroup>
```

## 2) `IGeoSpatialWriteRepository` implementasyonu
```csharp
using Oks.Location.Redis.Contracts;
using Oks.Location.Redis.Models;

public sealed class UserGeoWriteRepository : IGeoSpatialWriteRepository<UserLocation, Guid>
{
    public Task AddAsync(UserLocation entity, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public void Update(UserLocation entity) { }
    public void Remove(UserLocation entity) { }

    public IQueryable<UserLocation> Query() => throw new NotImplementedException();
    public Task<UserLocation?> GetAsync(Expression<Func<UserLocation, bool>> predicate, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public Task<UserLocation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public Task<List<UserLocation>> GetListAsync(Expression<Func<UserLocation, bool>>? predicate = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();

    public Task UpdatePositionAsync(UserLocation entity, GeoCoordinate point, CancellationToken cancellationToken = default)
    {
        entity.SetCoordinates(point.Latitude, point.Longitude);
        Update(entity);
        return Task.CompletedTask;
    }
}
```

## 3) Geo matcher servisi
```csharp
using Microsoft.Extensions.Caching.Distributed;
using Oks.Location.Redis.Contracts;
using Oks.Location.Redis.Models;

public sealed class GeoMatcher : IGeoSpatialMatcher
{
    private readonly IGeoSpatialCache _geoCache;

    public GeoMatcher(IGeoSpatialCache geoCache, IDistributedCache distributedCache)
    {
        _geoCache = geoCache;
        DistributedCache = distributedCache;
    }

    public IDistributedCache DistributedCache { get; }

    public Task<IReadOnlyCollection<GeoRadiusMatch>> FindNearbyAsync(
        string geoKey,
        GeoCoordinate center,
        double radiusInMeters,
        int take = 50,
        CancellationToken cancellationToken = default)
        => _geoCache.SearchRadiusAsync(geoKey, center, radiusInMeters, take, cancellationToken);
}
```

## Migration notu
- Eski isimler (`ILocationWriteRepository`, `ILocationGeoCache`, `IProximityMatcher`, `GeoPoint`, `ProximityMatch`) **çalışmaya devam eder**.
- Yeni geliştirmelerde `IGeoSpatial*` ve `GeoCoordinate/GeoRadiusMatch` kullanılması önerilir.
