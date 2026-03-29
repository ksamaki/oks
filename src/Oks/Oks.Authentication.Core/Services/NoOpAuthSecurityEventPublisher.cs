using Oks.Authentication.Abstractions.Contracts;

namespace Oks.Authentication.Core.Services;

public sealed class NoOpAuthSecurityEventPublisher : IAuthSecurityEventPublisher
{
    public Task PublishAsync(AuthSecurityEvent securityEvent, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
