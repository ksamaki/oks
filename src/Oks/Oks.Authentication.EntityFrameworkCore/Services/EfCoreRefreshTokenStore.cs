using Microsoft.EntityFrameworkCore;
using Oks.Authentication.Abstractions.Contracts;
using Oks.Authentication.EntityFrameworkCore.Entities;
using Oks.Authentication.EntityFrameworkCore.Persistence;

namespace Oks.Authentication.EntityFrameworkCore.Services;

public sealed class EfCoreRefreshTokenStore(OksAuthenticationDbContext dbContext) : IRefreshTokenStore
{
    public async Task PersistIssuedTokenAsync(Guid sessionId, Guid userId, Guid clientId, string refreshTokenHash, DateTime expiresAtUtc, string? tenantId, CancellationToken cancellationToken = default)
    {
        var entity = new OksRefreshToken
        {
            SessionId = sessionId,
            UserId = userId,
            ClientId = clientId,
            TokenHash = refreshTokenHash,
            ExpiresAtUtc = expiresAtUtc,
            TenantId = tenantId,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.RefreshTokens.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<RefreshTokenValidationResult?> GetValidationResultAsync(string refreshTokenHash, string clientCode, CancellationToken cancellationToken = default)
    {
        return await dbContext.RefreshTokens
            .Join(dbContext.Clients,
                token => token.ClientId,
                client => client.Id,
                (token, client) => new { token, client })
            .Where(x => x.token.TokenHash == refreshTokenHash && x.client.ClientId == clientCode)
            .Select(x => new RefreshTokenValidationResult(
                x.token.Id,
                x.token.SessionId,
                x.token.UserId,
                x.token.ClientId,
                x.client.ClientId,
                x.token.TenantId,
                x.token.ExpiresAtUtc,
                x.token.RevokedAtUtc.HasValue,
                x.token.ConsumedAtUtc.HasValue))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task RevokeAsync(Guid refreshTokenId, string reason, CancellationToken cancellationToken = default)
    {
        var token = await dbContext.RefreshTokens.FirstOrDefaultAsync(x => x.Id == refreshTokenId, cancellationToken);
        if (token is null)
            return;

        token.RevokedAtUtc = DateTime.UtcNow;
        token.RevokeReason = reason;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RotateAsync(Guid refreshTokenId, string newRefreshTokenHash, DateTime newExpiresAtUtc, CancellationToken cancellationToken = default)
    {
        var token = await dbContext.RefreshTokens.FirstOrDefaultAsync(x => x.Id == refreshTokenId, cancellationToken);
        if (token is null)
            return;

        token.ConsumedAtUtc = DateTime.UtcNow;
        token.ReplacedByTokenHash = newRefreshTokenHash;

        dbContext.RefreshTokens.Add(new OksRefreshToken
        {
            SessionId = token.SessionId,
            UserId = token.UserId,
            ClientId = token.ClientId,
            TokenHash = newRefreshTokenHash,
            ExpiresAtUtc = newExpiresAtUtc,
            TenantId = token.TenantId,
            CreatedAt = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
