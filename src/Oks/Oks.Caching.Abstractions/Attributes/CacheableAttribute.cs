namespace Oks.Caching.Abstractions.Attributes;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public sealed class CacheableAttribute : Attribute
{
    public string KeyTemplate { get; init; } = string.Empty;

    public int DurationSeconds { get; init; } = 300;

    public string[] Tags { get; init; } = Array.Empty<string>();
}
