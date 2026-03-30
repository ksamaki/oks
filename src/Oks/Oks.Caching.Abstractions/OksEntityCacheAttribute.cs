namespace Oks.Caching.Abstractions;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class OksEntityCacheAttribute : Attribute
{
    public int TtlSeconds { get; init; } = 120;

    public string[] Tags { get; init; } = Array.Empty<string>();

    public bool CacheEmptyResult { get; init; }
}
