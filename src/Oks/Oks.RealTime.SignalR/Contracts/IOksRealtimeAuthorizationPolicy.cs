namespace Oks.RealTime.SignalR.Contracts;

public interface IOksRealtimeAuthorizationPolicy
{
    Task<bool> CanConnectAsync(string userId, string? hubName, CancellationToken cancellationToken = default);

    Task<bool> CanJoinGroupAsync(string userId, string groupName, CancellationToken cancellationToken = default);
}
