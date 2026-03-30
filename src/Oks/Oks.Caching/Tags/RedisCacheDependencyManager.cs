using Oks.Caching.Abstractions;
using StackExchange.Redis;

namespace Oks.Caching.Tags;

public sealed class RedisCacheDependencyManager(IConnectionMultiplexer connectionMultiplexer) : ICacheDependencyManager
{
    private const string TagPrefix = "oks:cache:tag:";
    private IDatabase Database => connectionMultiplexer.GetDatabase();

    public async Task MapAsync(string key, IEnumerable<string> tags, CancellationToken cancellationToken = default)
    {
        foreach (var tag in tags.Where(static t => !string.IsNullOrWhiteSpace(t)).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            await Database.SetAddAsync(ToTagKey(tag), key).ConfigureAwait(false);
        }
    }

    public async Task<IReadOnlyCollection<string>> ResolveKeysAsync(string tagPattern, CancellationToken cancellationToken = default)
    {
        if (!tagPattern.Contains('*'))
        {
            var values = await Database.SetMembersAsync(ToTagKey(tagPattern)).ConfigureAwait(false);
            return values.Select(static x => x.ToString()).Where(static x => !string.IsNullOrWhiteSpace(x)).ToArray();
        }

        var endpoints = connectionMultiplexer.GetEndPoints();
        if (endpoints.Length == 0)
            return Array.Empty<string>();

        var server = connectionMultiplexer.GetServer(endpoints[0]);
        var redisPattern = ToTagKey(tagPattern);
        var keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        await foreach (var tagKey in server.KeysAsync(pattern: redisPattern).ConfigureAwait(false))
        {
            var members = await Database.SetMembersAsync(tagKey).ConfigureAwait(false);
            foreach (var member in members)
                keys.Add(member.ToString());
        }

        return keys.ToArray();
    }

    public Task RemoveKeyAsync(string key, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public async Task RemoveTagAsync(string tag, CancellationToken cancellationToken = default)
    {
        if (!tag.Contains('*'))
        {
            await Database.KeyDeleteAsync(ToTagKey(tag)).ConfigureAwait(false);
            return;
        }

        var endpoints = connectionMultiplexer.GetEndPoints();
        if (endpoints.Length == 0)
            return;

        var server = connectionMultiplexer.GetServer(endpoints[0]);
        await foreach (var tagKey in server.KeysAsync(pattern: ToTagKey(tag)).ConfigureAwait(false))
        {
            await Database.KeyDeleteAsync(tagKey).ConfigureAwait(false);
        }
    }

    private static string ToTagKey(string tag) => $"{TagPrefix}{tag}";
}
