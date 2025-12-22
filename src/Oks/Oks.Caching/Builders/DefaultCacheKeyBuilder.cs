namespace Oks.Caching.Builders;

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Oks.Caching.Abstractions.Interfaces;
using Oks.Caching.Abstractions.Models;

public sealed class DefaultCacheKeyBuilder : ICacheKeyBuilder
{
    public CacheKey BuildFromTemplate(string template, object? routeValues, object? queryValues = null, string? userId = null, string? tenantId = null)
    {
        var segments = new List<string> { "oks" };

        if (!string.IsNullOrWhiteSpace(tenantId))
        {
            segments.Add($"tenant:{tenantId}");
        }

        if (!string.IsNullOrWhiteSpace(userId))
        {
            segments.Add($"user:{userId}");
        }

        segments.Add(template);

        var payload = new
        {
            route = routeValues,
            query = queryValues
        };

        var hash = CreateDeterministicHash(payload);
        segments.Add(hash);

        return BuildFromSegments([.. segments]);
    }

    public CacheKey BuildFromSegments(params string[] segments)
    {
        var normalizedSegments = segments
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(NormalizeSegment);

        return new CacheKey(string.Join(":", normalizedSegments));
    }

    private static string CreateDeterministicHash(object? payload)
    {
        var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        });

        var bytes = Encoding.UTF8.GetBytes(json);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static string NormalizeSegment(string segment)
    {
        var trimmed = segment.Trim();
        return trimmed.Replace(" ", string.Empty).ToLowerInvariant();
    }
}
