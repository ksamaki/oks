using Microsoft.EntityFrameworkCore;
using Oks.Authentication.EntityFrameworkCore.Entities;
using Oks.Authentication.EntityFrameworkCore.Persistence;

namespace Oks.Authentication.EntityFrameworkCore.Services;

public sealed class OksAuthenticationSeeder(OksAuthenticationDbContext dbContext)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var adminRole = await EnsureRoleAsync("Admin", cancellationToken);
        var permissions = await EnsurePermissionsAsync(cancellationToken);

        foreach (var permission in permissions)
        {
            var exists = await dbContext.RolePermissions
                .AnyAsync(x => x.RoleId == adminRole.Id && x.PermissionId == permission.Id, cancellationToken);

            if (!exists)
            {
                dbContext.RolePermissions.Add(new OksRolePermission
                {
                            RoleId = adminRole.Id,
                    PermissionId = permission.Id
                });
            }
        }

        await EnsureClientAsync("Admin Panel", "oks_admin_web", "spa_web", "[\"authorization_code\",\"refresh_token\"]", "[\"openid\",\"profile\",\"oks_api\"]", cancellationToken);
        await EnsureClientAsync("Internal Service", "oks_internal_svc", "internal_service", "[\"client_credentials\"]", "[\"oks_internal\"]", cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<OksRole> EnsureRoleAsync(string roleName, CancellationToken cancellationToken)
    {
        var normalized = roleName.ToUpperInvariant();
        var existing = await dbContext.Roles.FirstOrDefaultAsync(x => x.NormalizedName == normalized, cancellationToken);
        if (existing is not null)
            return existing;

        var role = new OksRole
        {
            Name = roleName,
            NormalizedName = normalized,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.Roles.Add(role);
        await dbContext.SaveChangesAsync(cancellationToken);
        return role;
    }

    private async Task<List<OksPermission>> EnsurePermissionsAsync(CancellationToken cancellationToken)
    {
        var required = new[]
        {
            ("Authentication.Login", "auth.login"),
            ("Authentication.Refresh", "auth.refresh"),
            ("Users.Manage", "users.manage"),
            ("Roles.Manage", "roles.manage")
        };

        var result = new List<OksPermission>();
        foreach (var (name, code) in required)
        {
            var existing = await dbContext.Permissions.FirstOrDefaultAsync(x => x.Code == code, cancellationToken);
            if (existing is null)
            {
                existing = new OksPermission
                {
                            Name = name,
                    Code = code,
                    CreatedAt = DateTime.UtcNow
                };

                dbContext.Permissions.Add(existing);
                await dbContext.SaveChangesAsync(cancellationToken);
            }

            result.Add(existing);
        }

        return result;
    }

    private async Task EnsureClientAsync(string name, string clientId, string clientType, string grantsJson, string scopesJson, CancellationToken cancellationToken)
    {
        var exists = await dbContext.Clients.AnyAsync(x => x.ClientId == clientId, cancellationToken);
        if (exists)
            return;

        dbContext.Clients.Add(new OksClient
        {
            Name = name,
            ClientId = clientId,
            ClientType = clientType,
            AllowedGrantTypesJson = grantsJson,
            AllowedScopesJson = scopesJson,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        });
    }
}
