namespace Oks.Authentication.Abstractions.Models;

public sealed record LoginRequest(
    string UserName,
    string Password,
    string ClientId,
    string? ClientSecret,
    string? IpAddress,
    string? UserAgent);

public sealed record TokenPair(
    string AccessToken,
    DateTime AccessTokenExpiresAtUtc,
    string RefreshToken,
    DateTime RefreshTokenExpiresAtUtc,
    string TokenType = "Bearer");

public sealed record RefreshTokenRequest(
    string RefreshToken,
    string ClientId,
    string? IpAddress,
    string? UserAgent);

public sealed record LogoutRequest(
    Guid SessionId,
    Guid? UserId,
    string? Reason);

public sealed record AuthenticatedUser(
    Guid UserId,
    string UserName,
    string? TenantId,
    IReadOnlyCollection<string> Roles,
    IReadOnlyCollection<string> Permissions,
    IReadOnlyCollection<KeyValuePair<string, string>> Claims);

public sealed record ClientContext(
    Guid ClientId,
    string Code,
    string Name,
    string ClientType,
    bool IsActive,
    IReadOnlyCollection<string> AllowedGrantTypes,
    IReadOnlyCollection<string> AllowedScopes);
