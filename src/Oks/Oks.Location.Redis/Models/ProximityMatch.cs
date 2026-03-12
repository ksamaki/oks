namespace Oks.Location.Redis.Models;

public sealed record ProximityMatch(string MemberId, double DistanceInMeters);
