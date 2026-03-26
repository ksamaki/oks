using Microsoft.Extensions.Caching.Distributed;
using Oks.Location.Redis.Models;

namespace Oks.Location.Redis.Contracts;

public interface IGeoSpatialMatcher
{
    IDistributedCache DistributedCache { get; }

    Task<IReadOnlyCollection<GeoRadiusMatch>> FindNearbyAsync(
        string geoKey,
        GeoCoordinate center,
        double radiusInMeters,
        int take = 50,
        CancellationToken cancellationToken = default);
}
