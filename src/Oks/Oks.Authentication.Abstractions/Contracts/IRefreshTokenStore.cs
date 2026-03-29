namespace Oks.Authentication.Abstractions.Contracts;

public interface IRefreshTokenStore
{
    Task PersistIssuedTokenAsync(Guid sessionId, Guid userId, Guid clientId, string refreshTokenHash, DateTime expiresAtUtc, string? tenantId, CancellationToken cancellationToken = default);
    Task<RefreshTokenValidationResult?> GetValidationResultAsync(string refreshTokenHash, string clientCode, CancellationToken cancellationToken = default);
    Task RevokeAsync(Guid refreshTokenId, string reason, CancellationToken cancellationToken = default);
    Task RotateAsync(Guid refreshTokenId, string newRefreshTokenHash, DateTime newExpiresAtUtc, CancellationToken cancellationToken = default);
}

public sealed record RefreshTokenValidationResult(
    Guid RefreshTokenId,
    Guid SessionId,
    Guid UserId,
    Guid ClientId,
    string ClientCode,
    string? TenantId,
    DateTime ExpiresAtUtc,
    bool IsRevoked,
    bool IsConsumed);
