using System.Text;
using System.Security.Cryptography;

namespace Oks.Caching.Abstractions;

public sealed class CacheKey
{
    public CacheKey(IEnumerable<string> segments)
    {
        Segments = segments.ToArray();
    }

    public IReadOnlyList<string> Segments { get; }

    public string Value => string.Join(":", Segments);

    public static string Hash(object? payload)
    {
        if (payload is null)
            return "empty";

        var json = payload as string ?? System.Text.Json.JsonSerializer.Serialize(payload);
        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(json);
        var hashed = sha.ComputeHash(bytes);
        return Convert.ToHexString(hashed).ToLowerInvariant();
    }
}
