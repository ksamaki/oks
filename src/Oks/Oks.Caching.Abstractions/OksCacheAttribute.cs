namespace Oks.Caching.Abstractions;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class OksCacheAttribute : Attribute
{
    public string Key { get; init; } = string.Empty;

    public string[] Tags { get; init; } = Array.Empty<string>();

    public int? TtlSeconds { get; init; }

    public bool CacheEmptyResult { get; init; }

    public bool StampedeProtection { get; init; } = true;
}
