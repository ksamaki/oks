using Oks.Location.Redis.Models;

namespace Oks.Location.Redis.Contracts;

public interface IGeoSpatialCache
{
    Task SetPositionAsync(
        string geoKey,
        string memberId,
        GeoCoordinate point,
        CancellationToken cancellationToken = default);

    Task RemovePositionAsync(
        string geoKey,
        string memberId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<GeoRadiusMatch>> SearchRadiusAsync(
        string geoKey,
        GeoCoordinate center,
        double radiusInMeters,
        int take = 50,
        CancellationToken cancellationToken = default);
}
