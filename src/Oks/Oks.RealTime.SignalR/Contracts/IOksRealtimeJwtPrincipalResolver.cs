namespace Oks.RealTime.SignalR.Contracts;

public interface IOksRealtimeJwtPrincipalResolver
{
    Task<string?> ResolveUserIdAsync(string accessToken, CancellationToken cancellationToken = default);
}
