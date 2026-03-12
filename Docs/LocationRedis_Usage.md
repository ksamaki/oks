# Location Redis - Usage

[Location Redis - Description](LocationRedis_Description.md) | [Ana sayfa](../README.md)

Aşağıdaki örnek, `Oks.Location.Redis` kontratlarını WaitMe proximity senaryosunda nasıl kullanabileceğini gösterir.

## 1) Proje referansları (`.csproj`)
```xml
<ItemGroup>
  <ProjectReference Include="..\src\Oks\Oks.Location.Redis\Oks.Location.Redis.csproj" />
  <ProjectReference Include="..\src\Oks\Oks.Persistence.Abstractions\Oks.Persistence.Abstractions.csproj" />
</ItemGroup>
```

## 2) `ILocationWriteRepository` implementasyonu (örnek iskelet)
```csharp
using Oks.Location.Redis.Contracts;
using Oks.Location.Redis.Models;

public sealed class UserLocationWriteRepository : ILocationWriteRepository<UserLocation, Guid>
{
    public Task AddAsync(UserLocation entity, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public void Update(UserLocation entity) { }
    public void Remove(UserLocation entity) { }

    // IReadRepository üyeleri burada mevcut repository standardına göre implement edilir.

    public Task UpdateLocationAsync(UserLocation entity, GeoPoint point, CancellationToken cancellationToken = default)
    {
        entity.SetCoordinates(point.Latitude, point.Longitude);
        Update(entity);
        return Task.CompletedTask;
    }
}
```

## 3) Proximity matcher servisi (örnek)
```csharp
using Microsoft.Extensions.Caching.Distributed;
using Oks.Location.Redis.Contracts;
using Oks.Location.Redis.Models;

public sealed class WaitMeProximityMatcher : IProximityMatcher
{
    private readonly ILocationGeoCache _geoCache;

    public WaitMeProximityMatcher(ILocationGeoCache geoCache, IDistributedCache distributedCache)
    {
        _geoCache = geoCache;
        DistributedCache = distributedCache;
    }

    public IDistributedCache DistributedCache { get; }

    public Task<IReadOnlyCollection<ProximityMatch>> FindNearbyAsync(
        string geoKey,
        GeoPoint center,
        double radiusInMeters,
        int take = 50,
        CancellationToken cancellationToken = default)
        => _geoCache.SearchRadiusAsync(geoKey, center, radiusInMeters, take, cancellationToken);
}
```

## 4) Uygulama servisinde kullanım (örnek)
```csharp
public sealed class NearbyUsersQueryService
{
    private readonly IProximityMatcher _matcher;

    public NearbyUsersQueryService(IProximityMatcher matcher)
    {
        _matcher = matcher;
    }

    public Task<IReadOnlyCollection<ProximityMatch>> ExecuteAsync(double lat, double lng, CancellationToken cancellationToken)
        => _matcher.FindNearbyAsync("waitme:users", new GeoPoint(lat, lng), radiusInMeters: 2_000, take: 100, cancellationToken);
}
```
