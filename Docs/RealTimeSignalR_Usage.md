# Real-Time SignalR - Usage

[Real-Time SignalR - Description](RealTimeSignalR_Description.md) | [Ana sayfa](../README.md)

Aşağıdaki örnek, `Oks.RealTime.SignalR` kontratlarını WaitMe içinde nasıl kullanabileceğini gösterir.

## 1) Proje referansı (`.csproj`)
```xml
<ItemGroup>
  <ProjectReference Include="..\src\Oks\Oks.RealTime.SignalR\Oks.RealTime.SignalR.csproj" />
</ItemGroup>
```

## 2) JWT resolver implementasyonu (örnek)
```csharp
using Oks.RealTime.SignalR.Contracts;

public sealed class WaitMeJwtPrincipalResolver : IOksRealtimeJwtPrincipalResolver
{
    public Task<string?> ResolveUserIdAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        // Token doğrulama + claim okuma burada yapılır (sub/nameidentifier vb.)
        var userId = "user-123";
        return Task.FromResult<string?>(userId);
    }
}
```

## 3) Hub policy implementasyonu (örnek)
```csharp
using Oks.RealTime.SignalR.Contracts;

public sealed class WaitMeRealtimeAuthorizationPolicy : IOksRealtimeAuthorizationPolicy
{
    public Task<bool> CanConnectAsync(string userId, string? hubName, CancellationToken cancellationToken = default)
        => Task.FromResult(true);

    public Task<bool> CanJoinGroupAsync(string userId, string groupName, CancellationToken cancellationToken = default)
    {
        // Örn: kullanıcı yalnızca kendi lokasyon grubuna girebilsin
        var allowed = groupName.StartsWith("location:", StringComparison.OrdinalIgnoreCase);
        return Task.FromResult(allowed);
    }
}
```

## 4) Connection lifecycle servisi (örnek)
```csharp
using Oks.RealTime.SignalR.Contracts;
using Oks.RealTime.SignalR.Models;

public sealed class WaitMeHubSessionService : IOksRealtimeHubSessionService
{
    private readonly IOksRealtimeJwtPrincipalResolver _jwtResolver;
    private readonly IOksRealtimeAuthorizationPolicy _policy;
    private readonly IOksRealtimeConnectionStore _connectionStore;

    public WaitMeHubSessionService(
        IOksRealtimeJwtPrincipalResolver jwtResolver,
        IOksRealtimeAuthorizationPolicy policy,
        IOksRealtimeConnectionStore connectionStore)
    {
        _jwtResolver = jwtResolver;
        _policy = policy;
        _connectionStore = connectionStore;
    }

    public async Task OnConnectedAsync(string connectionId, string accessToken, string? hubName, string? deviceId, CancellationToken cancellationToken = default)
    {
        var userId = await _jwtResolver.ResolveUserIdAsync(accessToken, cancellationToken);
        if (string.IsNullOrWhiteSpace(userId))
            throw new UnauthorizedAccessException("Geçersiz JWT");

        var canConnect = await _policy.CanConnectAsync(userId, hubName, cancellationToken);
        if (!canConnect)
            throw new UnauthorizedAccessException("Hub bağlantı yetkisi yok");

        var connection = new OksRealtimeConnection(
            connectionId,
            userId,
            deviceId,
            DateTimeOffset.UtcNow,
            Array.Empty<string>());

        await _connectionStore.UpsertAsync(connection, cancellationToken);
    }

    public Task OnDisconnectedAsync(string connectionId, CancellationToken cancellationToken = default)
        => _connectionStore.RemoveAsync(connectionId, cancellationToken);
}
```
