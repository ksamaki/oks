namespace Oks.RealTime.SignalR.Models;

public sealed record OksRealtimeConnection(
    string ConnectionId,
    string UserId,
    string? DeviceId,
    DateTimeOffset ConnectedAt,
    IReadOnlyCollection<string> Groups);
