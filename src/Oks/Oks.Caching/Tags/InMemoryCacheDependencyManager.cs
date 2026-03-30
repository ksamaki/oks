using System.Collections.Concurrent;
using Oks.Caching.Abstractions;

namespace Oks.Caching.Tags;

public sealed class InMemoryCacheDependencyManager : ICacheDependencyManager
{
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, byte>> _tagToKeys = new(StringComparer.OrdinalIgnoreCase);

    public Task MapAsync(string key, IEnumerable<string> tags, CancellationToken cancellationToken = default)
    {
        foreach (var tag in tags.Where(static t => !string.IsNullOrWhiteSpace(t)).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var map = _tagToKeys.GetOrAdd(tag, _ => new ConcurrentDictionary<string, byte>(StringComparer.OrdinalIgnoreCase));
            map[key] = 0;
        }

        return Task.CompletedTask;
    }

    public Task<IReadOnlyCollection<string>> ResolveKeysAsync(string tagPattern, CancellationToken cancellationToken = default)
    {
        if (!tagPattern.Contains('*'))
        {
            if (_tagToKeys.TryGetValue(tagPattern, out var keys))
                return Task.FromResult<IReadOnlyCollection<string>>(keys.Keys.ToArray());

            return Task.FromResult<IReadOnlyCollection<string>>(Array.Empty<string>());
        }

        var prefix = tagPattern.TrimEnd('*');
        var matched = _tagToKeys
            .Where(kvp => kvp.Key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            .SelectMany(kvp => kvp.Value.Keys)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return Task.FromResult<IReadOnlyCollection<string>>(matched);
    }

    public Task RemoveKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        foreach (var kvp in _tagToKeys)
        {
            kvp.Value.TryRemove(key, out _);
            if (kvp.Value.IsEmpty)
                _tagToKeys.TryRemove(kvp.Key, out _);
        }

        return Task.CompletedTask;
    }

    public Task RemoveTagAsync(string tag, CancellationToken cancellationToken = default)
    {
        if (!tag.Contains('*'))
        {
            _tagToKeys.TryRemove(tag, out _);
            return Task.CompletedTask;
        }

        var prefix = tag.TrimEnd('*');
        foreach (var key in _tagToKeys.Keys.Where(k => k.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)).ToArray())
            _tagToKeys.TryRemove(key, out _);

        return Task.CompletedTask;
    }
}
