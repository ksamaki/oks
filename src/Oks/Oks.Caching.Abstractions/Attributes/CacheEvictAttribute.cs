namespace Oks.Caching.Abstractions.Attributes;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public sealed class CacheEvictAttribute : Attribute
{
    public string[] Tags { get; init; } = Array.Empty<string>();

    public bool EvictAllEntityCache { get; init; }
}
