namespace Oks.Location.Redis.Models;

[Obsolete("Use GeoRadiusMatch instead. ProximityMatch is kept for backward compatibility.")]
public sealed record ProximityMatch(string MemberId, double DistanceInMeters)
{
    public static implicit operator GeoRadiusMatch(ProximityMatch match) =>
        new(match.MemberId, match.DistanceInMeters);

    public static implicit operator ProximityMatch(GeoRadiusMatch match) =>
        new(match.MemberId, match.DistanceInMeters);
}
