namespace Oks.Caching.Serializers;

using System.Text.Json;
using Oks.Caching.Abstractions.Interfaces;

public sealed class DefaultCacheSerializer : ICacheSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    public T? Deserialize<T>(byte[]? payload)
    {
        if (payload is null || payload.Length == 0)
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(payload, Options);
    }

    public byte[] Serialize<T>(T value) => JsonSerializer.SerializeToUtf8Bytes(value, Options);
}
