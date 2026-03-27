namespace Oks.Caching.Abstractions;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public sealed class CustomCacheAttribute : Attribute
{
    public string KeyTemplate { get; init; } = string.Empty;

    public int DurationSeconds { get; init; } = 300;

    public string[] Tags { get; init; } = Array.Empty<string>();

    public bool Evict { get; init; }

    public bool EvictAllEntityCache { get; init; }
}
