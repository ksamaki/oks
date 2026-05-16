using Microsoft.EntityFrameworkCore.ChangeTracking;
using Oks.Domain.Base;

namespace Oks.Persistence.EfCore;

internal static class EntityGuidIdGenerator
{
    public static void AssignPendingGuids(ChangeTracker changeTracker)
    {
        foreach (var entry in changeTracker.Entries())
        {
            if (entry.State != Microsoft.EntityFrameworkCore.EntityState.Added || entry.Entity is null)
            {
                continue;
            }

            var idProperty = entry.Entity.GetType().GetProperty(nameof(Entity<Guid>.Id));
            if (idProperty?.PropertyType != typeof(Guid))
            {
                continue;
            }

            var currentValue = (Guid?)idProperty.GetValue(entry.Entity) ?? Guid.Empty;
            if (currentValue != Guid.Empty)
            {
                continue;
            }

            var setter = idProperty.SetMethod ?? idProperty.GetSetMethod(nonPublic: true);
            setter?.Invoke(entry.Entity, [Guid.NewGuid()]);
        }
    }
}
