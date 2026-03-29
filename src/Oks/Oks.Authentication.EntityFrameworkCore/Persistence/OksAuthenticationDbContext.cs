using Microsoft.EntityFrameworkCore;
using Oks.Authentication.EntityFrameworkCore.Entities;
using Oks.Authentication.EntityFrameworkCore.Options;

namespace Oks.Authentication.EntityFrameworkCore.Persistence;

public class OksAuthenticationDbContext(DbContextOptions<OksAuthenticationDbContext> options, OksAuthenticationEfCoreOptions efOptions) : DbContext(options)
{
    public string Schema => efOptions.Schema;

    public DbSet<OksUser> Users => Set<OksUser>();
    public DbSet<OksRole> Roles => Set<OksRole>();
    public DbSet<OksPermission> Permissions => Set<OksPermission>();
    public DbSet<OksRolePermission> RolePermissions => Set<OksRolePermission>();
    public DbSet<OksUserRole> UserRoles => Set<OksUserRole>();
    public DbSet<OksUserClaim> UserClaims => Set<OksUserClaim>();
    public DbSet<OksClient> Clients => Set<OksClient>();
    public DbSet<OksRefreshToken> RefreshTokens => Set<OksRefreshToken>();
    public DbSet<OksUserSession> Sessions => Set<OksUserSession>();
    public DbSet<OksLoginAttempt> LoginAttempts => Set<OksLoginAttempt>();
    public DbSet<OksSecurityEvent> SecurityEvents => Set<OksSecurityEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OksAuthenticationDbContext).Assembly);
        modelBuilder.HasDefaultSchema(Schema);
        base.OnModelCreating(modelBuilder);
    }
}
