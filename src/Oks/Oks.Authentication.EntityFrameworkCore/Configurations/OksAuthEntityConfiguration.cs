using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Oks.Authentication.EntityFrameworkCore.Entities;

namespace Oks.Authentication.EntityFrameworkCore.Configurations;

public sealed class OksUserConfiguration : IEntityTypeConfiguration<OksUser>
{
    public void Configure(EntityTypeBuilder<OksUser> builder)
    {
        builder.ToTable("OksUser");
        builder.HasIndex(x => new { x.TenantId, x.NormalizedUserName }).IsUnique();
        builder.Property(x => x.UserName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.PasswordHash).HasMaxLength(512).IsRequired();
    }
}

public sealed class OksRoleConfiguration : IEntityTypeConfiguration<OksRole>
{
    public void Configure(EntityTypeBuilder<OksRole> builder)
    {
        builder.ToTable("OksRole");
        builder.HasIndex(x => new { x.TenantId, x.NormalizedName }).IsUnique();
        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
    }
}

public sealed class OksPermissionConfiguration : IEntityTypeConfiguration<OksPermission>
{
    public void Configure(EntityTypeBuilder<OksPermission> builder)
    {
        builder.ToTable("OksPermission");
        builder.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        builder.Property(x => x.Code).HasMaxLength(120).IsRequired();
    }
}

public sealed class OksRolePermissionConfiguration : IEntityTypeConfiguration<OksRolePermission>
{
    public void Configure(EntityTypeBuilder<OksRolePermission> builder)
    {
        builder.ToTable("OksRolePermission");
        builder.HasIndex(x => new { x.RoleId, x.PermissionId }).IsUnique();
        builder.HasOne(x => x.Role).WithMany(x => x.Permissions).HasForeignKey(x => x.RoleId);
        builder.HasOne(x => x.Permission).WithMany(x => x.Roles).HasForeignKey(x => x.PermissionId);
    }
}

public sealed class OksUserRoleConfiguration : IEntityTypeConfiguration<OksUserRole>
{
    public void Configure(EntityTypeBuilder<OksUserRole> builder)
    {
        builder.ToTable("OksUserRole");
        builder.HasIndex(x => new { x.UserId, x.RoleId }).IsUnique();
        builder.HasOne(x => x.User).WithMany(x => x.Roles).HasForeignKey(x => x.UserId);
        builder.HasOne(x => x.Role).WithMany(x => x.Users).HasForeignKey(x => x.RoleId);
    }
}

public sealed class OksUserClaimConfiguration : IEntityTypeConfiguration<OksUserClaim>
{
    public void Configure(EntityTypeBuilder<OksUserClaim> builder)
    {
        builder.ToTable("OksUserClaim");
        builder.Property(x => x.ClaimType).HasMaxLength(200).IsRequired();
        builder.Property(x => x.ClaimValue).HasMaxLength(1000).IsRequired();
    }
}

public sealed class OksClientConfiguration : IEntityTypeConfiguration<OksClient>
{
    public void Configure(EntityTypeBuilder<OksClient> builder)
    {
        builder.ToTable("OksClient");
        builder.HasIndex(x => new { x.TenantId, x.ClientId }).IsUnique();
        builder.Property(x => x.ClientId).HasMaxLength(120).IsRequired();
        builder.Property(x => x.ClientType).HasMaxLength(50).IsRequired();
    }
}

public sealed class OksRefreshTokenConfiguration : IEntityTypeConfiguration<OksRefreshToken>
{
    public void Configure(EntityTypeBuilder<OksRefreshToken> builder)
    {
        builder.ToTable("OksRefreshToken");
        builder.HasIndex(x => x.TokenHash).IsUnique();
        builder.HasIndex(x => new { x.TenantId, x.UserId, x.ClientId });
        builder.Property(x => x.TokenHash).HasMaxLength(512).IsRequired();
    }
}

public sealed class OksUserSessionConfiguration : IEntityTypeConfiguration<OksUserSession>
{
    public void Configure(EntityTypeBuilder<OksUserSession> builder)
    {
        builder.ToTable("OksUserSession");
        builder.HasIndex(x => new { x.TenantId, x.UserId, x.ClientId });
    }
}

public sealed class OksLoginAttemptConfiguration : IEntityTypeConfiguration<OksLoginAttempt>
{
    public void Configure(EntityTypeBuilder<OksLoginAttempt> builder)
    {
        builder.ToTable("OksLoginAttempt");
        builder.HasIndex(x => new { x.UserName, x.ClientId, x.CreatedAt });
    }
}

public sealed class OksSecurityEventConfiguration : IEntityTypeConfiguration<OksSecurityEvent>
{
    public void Configure(EntityTypeBuilder<OksSecurityEvent> builder)
    {
        builder.ToTable("OksSecurityEvent");
        builder.HasIndex(x => new { x.TenantId, x.EventType, x.CreatedAt });
        builder.Property(x => x.EventType).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Severity).HasMaxLength(20).IsRequired();
    }
}
