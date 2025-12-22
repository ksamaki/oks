namespace Oks.Caching.Abstractions.Interfaces;

public interface ICacheSerializer
{
    byte[] Serialize<T>(T value);

    T? Deserialize<T>(byte[]? payload);
}
