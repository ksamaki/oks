using Oks.RealTime.SignalR.Models;

namespace Oks.RealTime.SignalR.Contracts;

public interface IOksRealtimeConnectionStore
{
    Task UpsertAsync(OksRealtimeConnection connection, CancellationToken cancellationToken = default);

    Task RemoveAsync(string connectionId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<OksRealtimeConnection>> GetUserConnectionsAsync(
        string userId,
        CancellationToken cancellationToken = default);
}
