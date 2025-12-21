namespace Oks.Caching.Abstractions;

public interface ICacheKeyBuilder
{
    CacheKey FromTemplate(string template, object? parameters = null, string? tenantId = null, string? userId = null);

    CacheKey ForRead<TEntity>(string operation, object? parameters = null, string? tenantId = null, string? userId = null);
}
