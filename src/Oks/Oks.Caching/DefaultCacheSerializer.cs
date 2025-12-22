using System.Text.Json;
using Oks.Caching.Abstractions;

namespace Oks.Caching;

public sealed class DefaultCacheSerializer : ICacheSerializer
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public byte[] Serialize<T>(T value)
        => JsonSerializer.SerializeToUtf8Bytes(value, Options);

    public T? Deserialize<T>(byte[] bytes)
        => JsonSerializer.Deserialize<T>(bytes, Options);
}
