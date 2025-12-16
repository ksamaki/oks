using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Oks.Domain.Base;
using System.Linq.Expressions;
using System.Reflection;

namespace Oks.Persistence.EfCore;

public abstract class OksDbContextBase : DbContext
{
    protected OksDbContextBase(DbContextOptions options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ApplyGlobalQueryFilters(modelBuilder);
    }

    /// <summary>
    /// SaveChanges çağrılmadan hemen önce audit ve soft delete mantığını uygular.
    /// </summary>
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
        var currentUser = GetCurrentUserIdentifier();

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
                    // İlk oluştururken diğer alanları sıfırlayalım
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
        var currentUser = GetCurrentUserIdentifier();

        foreach (EntityEntry entry in ChangeTracker.Entries())
        {
            if (entry.Entity is not IAuditedEntity audited)
                continue;

            if (!audited.IsAuditEnabled)
                continue;

            if (entry.State == EntityState.Deleted)
            {
                // Hard delete yerine soft delete
                entry.State = EntityState.Modified;

                audited.IsDeleted = true;
                audited.DeletedAt = now;
                audited.DeletedBy = currentUser;
            }
        }
    }

    /// <summary>
    /// Mevcut kullanıcı bilgisini elde etmek için override edilebilir nokta.
    /// Bunu override edip HttpContext'ten kullanıcı adını/id'sini alabiliriz.
    /// </summary>
    protected virtual string? GetCurrentUserIdentifier() => null;

    #endregion

    #region Global Query Filters (IsDeleted == false)

    private static readonly MethodInfo EfPropertyMethod =
        typeof(EF).GetMethod(nameof(EF.Property))!
            .GetGenericMethodDefinition();

    private void ApplyGlobalQueryFilters(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;

            if (!typeof(IAuditedEntity).IsAssignableFrom(clrType))
                continue;

            var parameter = Expression.Parameter(clrType, "e");

            // e => !((IAuditedEntity)e).IsDeleted
            var prop = Expression.Property(
                Expression.Convert(parameter, typeof(IAuditedEntity)),
                nameof(IAuditedEntity.IsDeleted));

            var body = Expression.Equal(
                prop,
                Expression.Constant(false));

            var lambda = Expression.Lambda(body, parameter);

            modelBuilder.Entity(clrType).HasQueryFilter(lambda);
        }
    }

    #endregion
}
