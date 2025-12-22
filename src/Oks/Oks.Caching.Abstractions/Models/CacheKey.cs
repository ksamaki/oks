namespace Oks.Caching.Abstractions.Models;

public sealed record CacheKey
{
    public CacheKey(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public override string ToString() => Value;
}
