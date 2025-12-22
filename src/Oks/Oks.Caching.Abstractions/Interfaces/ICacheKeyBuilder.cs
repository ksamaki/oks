namespace Oks.Caching.Abstractions.Interfaces;

using Oks.Caching.Abstractions.Models;

public interface ICacheKeyBuilder
{
    CacheKey BuildFromTemplate(string template, object? routeValues, object? queryValues = null, string? userId = null, string? tenantId = null);

    CacheKey BuildFromSegments(params string[] segments);
}
