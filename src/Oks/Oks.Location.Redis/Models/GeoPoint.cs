namespace Oks.Location.Redis.Models;

[Obsolete("Use GeoCoordinate instead. GeoPoint is kept for backward compatibility.")]
public sealed record GeoPoint(double Latitude, double Longitude)
{
    public static implicit operator GeoCoordinate(GeoPoint point) =>
        new(point.Latitude, point.Longitude);

    public static implicit operator GeoPoint(GeoCoordinate point) =>
        new(point.Latitude, point.Longitude);
}
