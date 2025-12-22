using System.Collections.Concurrent;

namespace Oks.Caching.Tags;

public sealed class InMemoryCacheTagIndex : ICacheTagIndex
{
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, byte>> _tagToKeys = new();

    public void Map(CacheKey key, IEnumerable<string> tags)
    {
        foreach (var tag in tags)
        {
            var map = _tagToKeys.GetOrAdd(tag, _ => new ConcurrentDictionary<string, byte>());
            map[key.Value] = 0;
        }
    }

    public IReadOnlyCollection<string> KeysFor(string tag)
    {
        if (_tagToKeys.TryGetValue(tag, out var keys))
        {
            return keys.Keys.ToArray();
        }

        return Array.Empty<string>();
    }

    public void RemoveKey(string key)
    {
        foreach (var kvp in _tagToKeys)
        {
            kvp.Value.TryRemove(key, out _);

            if (kvp.Value.IsEmpty)
            {
                _tagToKeys.TryRemove(kvp.Key, out _);
            }
        }
    }

    public void RemoveTag(string tag)
    {
        _tagToKeys.TryRemove(tag, out _);
    }
}
