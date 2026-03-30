using Oks.Caching.Abstractions;

namespace Oks.Caching.Tags;

[Obsolete("Use ICacheDependencyManager instead.")]
public interface ICacheTagIndex
{
    void Map(CacheKey key, IEnumerable<string> tags);

    IReadOnlyCollection<string> KeysFor(string tag);

    void RemoveKey(string key);

    void RemoveTag(string tag);
}
