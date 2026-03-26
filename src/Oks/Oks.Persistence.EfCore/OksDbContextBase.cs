using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Oks.Domain.Base;
using Oks.Persistence.Abstractions;
using Oks.Persistence.EfCore.Users;
using System.Linq.Expressions;

namespace Oks.Persistence.EfCore;

public abstract class OksDbContextBase : DbContext
{
    private readonly IOksUserProvider? _explicitUserProvider;

    protected OksDbContextBase(DbContextOptions options)
        : base(options)
    {
    }

    // Geriye uyumluluk + test kolaylığı: isteyen explicit provider da verebilir.
    protected OksDbContextBase(
        DbContextOptions options,
        IOksUserProvider? userProvider)
        : base(options)
    {
        _explicitUserProvider = userProvider;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ApplyGlobalQueryFilters(modelBuilder);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditInfo();
        ApplySoftDelete();

        return base.SaveChangesAsync(cancellationToken);
    }

    #region Audit

    private void ApplyAuditInfo()
    {
        var now = DateTime.UtcNow;
        var currentUser = ResolveCurrentUserIdentifier();

        foreach (EntityEntry entry in ChangeTracker.Entries())
        {
            if (entry.Entity is not IAuditedEntity audited)
                continue;

            if (!audited.IsAuditEnabled)
                continue;

            switch (entry.State)
            {
                case EntityState.Added:
                    audited.CreatedAt = now;
                    audited.CreatedBy ??= currentUser;
                    audited.UpdatedAt = default;
                    audited.UpdatedBy = null;
                    audited.DeletedAt = null;
                    audited.DeletedBy = null;
                    audited.IsDeleted = false;
                    break;

                case EntityState.Modified:
                    audited.UpdatedAt = now;
                    audited.UpdatedBy = currentUser;
                    break;
            }
        }
    }

    private void ApplySoftDelete()
    {
        var now = DateTime.UtcNow;
        var currentUser = ResolveCurrentUserIdentifier();

        foreach (EntityEntry entry in ChangeTracker.Entries())
        {
            if (entry.Entity is not IAuditedEntity audited)
                continue;

            if (!audited.IsAuditEnabled)
                continue;

            if (entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;

                audited.IsDeleted = true;
                audited.DeletedAt = now;
                audited.DeletedBy = currentUser;
            }
        }
    }

    private string? ResolveCurrentUserIdentifier()
    {
        if (_explicitUserProvider is not null)
            return _explicitUserProvider.GetCurrentUserIdentifier();

        var scopedProvider = ((IInfrastructure<IServiceProvider>)this).Instance
            .GetService(typeof(IOksUserProvider)) as IOksUserProvider;

        return scopedProvider?.GetCurrentUserIdentifier()
               ?? NullOksUserProvider.Instance.GetCurrentUserIdentifier();
    }

    #endregion

    #region Global Query Filters (IsDeleted == false)

    private void ApplyGlobalQueryFilters(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;

            if (!typeof(IAuditedEntity).IsAssignableFrom(clrType))
                continue;

            var parameter = Expression.Parameter(clrType, "e");
            var prop = Expression.Property(
                Expression.Convert(parameter, typeof(IAuditedEntity)),
                nameof(IAuditedEntity.IsDeleted));

            var body = Expression.Equal(prop, Expression.Constant(false));
            var lambda = Expression.Lambda(body, parameter);

            modelBuilder.Entity(clrType).HasQueryFilter(lambda);
        }
    }

    #endregion
}
