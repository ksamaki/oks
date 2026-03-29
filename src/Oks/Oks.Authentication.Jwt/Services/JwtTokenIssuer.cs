using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Oks.Authentication.Abstractions.Contracts;
using Oks.Authentication.Abstractions.Models;
using Oks.Authentication.Core.Constants;
using Oks.Authentication.Jwt.Options;

namespace Oks.Authentication.Jwt.Services;

public sealed class JwtTokenIssuer(IOptions<OksJwtOptions> options) : ITokenIssuer
{
    private readonly OksJwtOptions _options = options.Value;

    public Task<TokenPair> IssueAsync(AuthenticatedUser user, ClientContext client, Guid sessionId, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var accessExpiry = now.AddMinutes(_options.AccessTokenMinutes);

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = BuildClaims(user, client, sessionId);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: now,
            expires: accessExpiry,
            signingCredentials: creds);

        var handler = new JwtSecurityTokenHandler();
        var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var refreshExpiry = now.AddDays(_options.RefreshTokenDays);

        return Task.FromResult(new TokenPair(
            handler.WriteToken(token),
            accessExpiry,
            refreshToken,
            refreshExpiry));
    }

    private static IEnumerable<Claim> BuildClaims(AuthenticatedUser user, ClientContext client, Guid sessionId)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
            new(OksClaimTypes.UserId, user.UserId.ToString()),
            new(OksClaimTypes.ClientId, client.ClientId.ToString()),
            new(OksClaimTypes.SessionId, sessionId.ToString())
        };

        if (!string.IsNullOrWhiteSpace(user.TenantId))
            claims.Add(new Claim(OksClaimTypes.TenantId, user.TenantId));

        claims.AddRange(user.Roles.Select(role => new Claim(ClaimTypes.Role, role)));
        claims.AddRange(user.Permissions.Select(permission => new Claim(OksClaimTypes.Permission, permission)));

        return claims;
    }
}
