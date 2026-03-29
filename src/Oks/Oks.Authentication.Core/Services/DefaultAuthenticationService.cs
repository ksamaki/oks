using Oks.Authentication.Abstractions.Contracts;
using Oks.Authentication.Abstractions.Models;

namespace Oks.Authentication.Core.Services;

public sealed class DefaultAuthenticationService(
    IClientStore clientStore,
    IUserCredentialValidator credentialValidator,
    ITokenIssuer tokenIssuer,
    IRefreshTokenStore refreshTokenStore,
    ISecretHasher secretHasher,
    IAuthSecurityEventPublisher securityEventPublisher) : IAuthenticationService
{
    public async Task<TokenPair> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var client = await clientStore.GetActiveClientAsync(request.ClientId, cancellationToken)
            ?? throw new InvalidOperationException("Client not found or passive.");

        var isClientSecretValid = await clientStore.ValidateSecretAsync(request.ClientId, request.ClientSecret, cancellationToken);
        if (!isClientSecretValid)
        {
            await PublishFailureAsync("client_secret_invalid", request.IpAddress, request.UserAgent, null, client.ClientId, cancellationToken);
            throw new UnauthorizedAccessException("Invalid client credentials.");
        }

        var user = await credentialValidator.ValidateAsync(request.UserName, request.Password, client.Code, cancellationToken)
            ?? throw new UnauthorizedAccessException("Invalid username or password.");

        var sessionId = Guid.NewGuid();
        var tokenPair = await tokenIssuer.IssueAsync(user, client, sessionId, cancellationToken);
        var refreshTokenHash = secretHasher.Hash(tokenPair.RefreshToken);

        await refreshTokenStore.PersistIssuedTokenAsync(
            sessionId,
            user.UserId,
            client.ClientId,
            refreshTokenHash,
            tokenPair.RefreshTokenExpiresAtUtc,
            user.TenantId,
            cancellationToken);

        return tokenPair;
    }

    public async Task<TokenPair> RefreshAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        var hashedRefreshToken = secretHasher.Hash(request.RefreshToken);
        var tokenRecord = await refreshTokenStore.GetValidationResultAsync(hashedRefreshToken, request.ClientId, cancellationToken)
            ?? throw new UnauthorizedAccessException("Refresh token not found.");

        if (tokenRecord.IsRevoked || tokenRecord.IsConsumed || tokenRecord.ExpiresAtUtc <= DateTime.UtcNow)
            throw new UnauthorizedAccessException("Refresh token is no longer valid.");

        await refreshTokenStore.RevokeAsync(tokenRecord.RefreshTokenId, "rotated", cancellationToken);

        var user = new AuthenticatedUser(tokenRecord.UserId, string.Empty, tokenRecord.TenantId, [], [], []);
        var client = new ClientContext(tokenRecord.ClientId, tokenRecord.ClientCode, tokenRecord.ClientCode, "unknown", true, [], []);

        var rotatedPair = await tokenIssuer.IssueAsync(user, client, tokenRecord.SessionId, cancellationToken);
        await refreshTokenStore.RotateAsync(
            tokenRecord.RefreshTokenId,
            secretHasher.Hash(rotatedPair.RefreshToken),
            rotatedPair.RefreshTokenExpiresAtUtc,
            cancellationToken);

        return rotatedPair;
    }

    public async Task LogoutAsync(LogoutRequest request, CancellationToken cancellationToken = default)
    {
        if (request.SessionId == Guid.Empty)
            return;

        await securityEventPublisher.PublishAsync(
            new AuthSecurityEvent(
                "logout",
                request.UserId,
                null,
                request.SessionId,
                "info",
                null,
                null,
                request.Reason,
                null,
                DateTime.UtcNow),
            cancellationToken);
    }

    private Task PublishFailureAsync(string eventType, string? ipAddress, string? userAgent, Guid? userId, Guid? clientId, CancellationToken cancellationToken)
    {
        return securityEventPublisher.PublishAsync(
            new AuthSecurityEvent(
                eventType,
                userId,
                clientId,
                null,
                "warning",
                ipAddress,
                userAgent,
                "Authentication failure captured by DefaultAuthenticationService",
                null,
                DateTime.UtcNow),
            cancellationToken);
    }
}
