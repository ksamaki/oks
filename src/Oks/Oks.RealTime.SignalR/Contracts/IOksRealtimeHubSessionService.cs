namespace Oks.RealTime.SignalR.Contracts;

public interface IOksRealtimeHubSessionService
{
    Task OnConnectedAsync(
        string connectionId,
        string accessToken,
        string? hubName,
        string? deviceId,
        CancellationToken cancellationToken = default);

    Task OnDisconnectedAsync(string connectionId, CancellationToken cancellationToken = default);
}
