namespace Oks.Authentication.Abstractions.Contracts;

public interface IAuthSecurityEventPublisher
{
    Task PublishAsync(AuthSecurityEvent securityEvent, CancellationToken cancellationToken = default);
}

public sealed record AuthSecurityEvent(
    string EventType,
    Guid? UserId,
    Guid? ClientId,
    Guid? SessionId,
    string Severity,
    string? IpAddress,
    string? UserAgent,
    string? Description,
    string? TenantId,
    DateTime OccurredAtUtc);
