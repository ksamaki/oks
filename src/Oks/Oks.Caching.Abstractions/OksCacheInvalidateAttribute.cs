namespace Oks.Caching.Abstractions;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public sealed class OksCacheInvalidateAttribute : Attribute
{
    public string[] Tags { get; init; } = Array.Empty<string>();
}
