using Microsoft.Extensions.Caching.Distributed;
using Oks.Location.Redis.Models;

namespace Oks.Location.Redis.Contracts;

public interface IProximityMatcher
{
    IDistributedCache DistributedCache { get; }

    Task<IReadOnlyCollection<ProximityMatch>> FindNearbyAsync(
        string geoKey,
        GeoPoint center,
        double radiusInMeters,
        int take = 50,
        CancellationToken cancellationToken = default);
}
