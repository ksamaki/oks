namespace Oks.Caching.Internal;

internal sealed class CacheEnvelope<T>
{
    public T? Value { get; init; }

    public DateTimeOffset CreatedAtUtc { get; init; }

    public TimeSpan? AbsoluteExpirationRelativeToNow { get; init; }

    public TimeSpan? SlidingExpiration { get; init; }

    public TimeSpan? SoftExpiration { get; init; }

    public string[] Tags { get; init; } = Array.Empty<string>();
}
