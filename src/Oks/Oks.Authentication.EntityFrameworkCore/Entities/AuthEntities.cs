using Oks.Domain.Base;

namespace Oks.Authentication.EntityFrameworkCore.Entities;

public interface ITenantAware
{
    string? TenantId { get; set; }
}

public sealed class OksUser : AuditedEntity<Guid>, ITenantAware
{
    public string UserName { get; set; } = string.Empty;
    public string NormalizedUserName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public string? TenantId { get; set; }

    public ICollection<OksUserRole> Roles { get; set; } = new List<OksUserRole>();
    public ICollection<OksUserClaim> Claims { get; set; } = new List<OksUserClaim>();
}

public sealed class OksRole : AuditedEntity<Guid>, ITenantAware
{
    public string Name { get; set; } = string.Empty;
    public string NormalizedName { get; set; } = string.Empty;
    public string? TenantId { get; set; }

    public ICollection<OksUserRole> Users { get; set; } = new List<OksUserRole>();
    public ICollection<OksRolePermission> Permissions { get; set; } = new List<OksRolePermission>();
}

public sealed class OksPermission : AuditedEntity<Guid>, ITenantAware
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? TenantId { get; set; }

    public ICollection<OksRolePermission> Roles { get; set; } = new List<OksRolePermission>();
}

public sealed class OksRolePermission : Entity<Guid>
{
    public Guid RoleId { get; set; }
    public Guid PermissionId { get; set; }

    public OksRole Role { get; set; } = default!;
    public OksPermission Permission { get; set; } = default!;
}

public sealed class OksUserRole : Entity<Guid>
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }

    public OksUser User { get; set; } = default!;
    public OksRole Role { get; set; } = default!;
}

public sealed class OksUserClaim : Entity<Guid>
{
    public Guid UserId { get; set; }
    public string ClaimType { get; set; } = string.Empty;
    public string ClaimValue { get; set; } = string.Empty;

    public OksUser User { get; set; } = default!;
}

public sealed class OksClient : AuditedEntity<Guid>, ITenantAware
{
    public string Name { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string? ClientSecretHash { get; set; }
    public string ClientType { get; set; } = string.Empty;
    public string AllowedGrantTypesJson { get; set; } = "[]";
    public string AllowedScopesJson { get; set; } = "[]";
    public string RedirectUrisJson { get; set; } = "[]";
    public bool IsActive { get; set; } = true;
    public string? TenantId { get; set; }
}

public sealed class OksRefreshToken : AuditedEntity<Guid>, ITenantAware
{
    public Guid UserId { get; set; }
    public Guid ClientId { get; set; }
    public Guid SessionId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
    public DateTime? RevokedAtUtc { get; set; }
    public DateTime? ConsumedAtUtc { get; set; }
    public string? RevokeReason { get; set; }
    public string? ReplacedByTokenHash { get; set; }
    public string? TenantId { get; set; }
}

public sealed class OksUserSession : AuditedEntity<Guid>, ITenantAware
{
    public Guid UserId { get; set; }
    public Guid ClientId { get; set; }
    public DateTime LastSeenAtUtc { get; set; }
    public DateTime? RevokedAtUtc { get; set; }
    public string? RevokedReason { get; set; }
    public string? TenantId { get; set; }
}

public sealed class OksLoginAttempt : AuditedEntity<Guid>, ITenantAware
{
    public string UserName { get; set; } = string.Empty;
    public Guid? UserId { get; set; }
    public string ClientId { get; set; } = string.Empty;
    public bool IsSucceeded { get; set; }
    public string? FailureReason { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? TenantId { get; set; }
}

public sealed class OksSecurityEvent : AuditedEntity<Guid>, ITenantAware
{
    public string EventType { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public Guid? UserId { get; set; }
    public Guid? ClientId { get; set; }
    public Guid? SessionId { get; set; }
    public string? Description { get; set; }
    public string? MetadataJson { get; set; }
    public string? TenantId { get; set; }
}
