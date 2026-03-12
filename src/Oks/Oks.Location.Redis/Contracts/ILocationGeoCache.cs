using Oks.Location.Redis.Models;

namespace Oks.Location.Redis.Contracts;

public interface ILocationGeoCache
{
    Task SetPositionAsync(
        string geoKey,
        string memberId,
        GeoPoint point,
        CancellationToken cancellationToken = default);

    Task RemovePositionAsync(
        string geoKey,
        string memberId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<ProximityMatch>> SearchRadiusAsync(
        string geoKey,
        GeoPoint center,
        double radiusInMeters,
        int take = 50,
        CancellationToken cancellationToken = default);
}
