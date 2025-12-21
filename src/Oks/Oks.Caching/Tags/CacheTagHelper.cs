using Oks.Domain.Base;

namespace Oks.Caching.Tags;

public static class CacheTagHelper
{
    public static IReadOnlyCollection<string> ForEntity<TEntity, TKey>(TEntity entity)
        where TEntity : Entity<TKey>
    {
        var entityName = typeof(TEntity).Name;
        var id = entity.Id?.ToString() ?? "0";

        return new[]
        {
            entityName,
            $"{entityName}:{id}",
            $"Query:{entityName}"
        };
    }

    public static IReadOnlyCollection<string> ForEntity<TEntity, TKey>(TKey id)
    {
        var entityName = typeof(TEntity).Name;
        var idValue = id?.ToString() ?? "0";

        return new[]
        {
            entityName,
            $"{entityName}:{idValue}",
            $"Query:{entityName}"
        };
    }

    public static IReadOnlyCollection<string> ForEntityName<TEntity>()
        => new[] { typeof(TEntity).Name, $"Query:{typeof(TEntity).Name}" };
}
