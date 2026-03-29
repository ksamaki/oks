using Oks.Authentication.Abstractions.Contracts;
using Oks.Authentication.EntityFrameworkCore.Entities;
using Oks.Authentication.EntityFrameworkCore.Persistence;

namespace Oks.Authentication.EntityFrameworkCore.Services;

public sealed class EfCoreAuthSecurityEventPublisher(OksAuthenticationDbContext dbContext) : IAuthSecurityEventPublisher
{
    public async Task PublishAsync(AuthSecurityEvent securityEvent, CancellationToken cancellationToken = default)
    {
        dbContext.SecurityEvents.Add(new OksSecurityEvent
        {
            EventType = securityEvent.EventType,
            Severity = securityEvent.Severity,
            UserId = securityEvent.UserId,
            ClientId = securityEvent.ClientId,
            SessionId = securityEvent.SessionId,
            Description = securityEvent.Description,
            TenantId = securityEvent.TenantId,
            CreatedAt = securityEvent.OccurredAtUtc
        });

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
