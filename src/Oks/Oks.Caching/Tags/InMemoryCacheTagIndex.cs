using Oks.Caching.Abstractions;

namespace Oks.Caching.Tags;

[Obsolete("Use InMemoryCacheDependencyManager instead.")]
public sealed class InMemoryCacheTagIndex(InMemoryCacheDependencyManager inner) : ICacheTagIndex
{
    public void Map(CacheKey key, IEnumerable<string> tags)
        => inner.MapAsync(key.Value, tags).GetAwaiter().GetResult();

    public IReadOnlyCollection<string> KeysFor(string tag)
        => inner.ResolveKeysAsync(tag).GetAwaiter().GetResult();

    public void RemoveKey(string key)
        => inner.RemoveKeyAsync(key).GetAwaiter().GetResult();

    public void RemoveTag(string tag)
        => inner.RemoveTagAsync(tag).GetAwaiter().GetResult();
}
