using Oks.Caching.Abstractions;

namespace Oks.Caching;

public class CacheKeyBuilder : ICacheKeyBuilder
{
    public CacheKey FromTemplate(
        string template,
        object? parameters = null,
        string? tenantId = null,
        string? userId = null)
    {
        var segments = BuildPrefix(tenantId, userId);
        segments.Add(template);

        if (parameters is not null)
        {
            segments.Add(CacheKey.Hash(parameters));
        }

        return new CacheKey(segments);
    }

    public CacheKey ForRead<TEntity>(
        string operation,
        object? parameters = null,
        string? tenantId = null,
        string? userId = null)
    {
        var segments = BuildPrefix(tenantId, userId);
        segments.Add(typeof(TEntity).Name);
        segments.Add(operation);

        if (parameters is not null)
        {
            segments.Add(CacheKey.Hash(parameters));
        }

        return new CacheKey(segments);
    }

    private static List<string> BuildPrefix(string? tenantId, string? userId)
    {
        var segments = new List<string> { "oks", "cache" };

        if (!string.IsNullOrWhiteSpace(tenantId))
        {
            segments.Add($"tenant:{tenantId}");
        }

        if (!string.IsNullOrWhiteSpace(userId))
        {
            segments.Add($"user:{userId}");
        }

        return segments;
    }
}
