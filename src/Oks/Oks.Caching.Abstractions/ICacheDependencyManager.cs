namespace Oks.Caching.Abstractions;

public interface ICacheDependencyManager
{
    Task MapAsync(string key, IEnumerable<string> tags, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<string>> ResolveKeysAsync(string tagPattern, CancellationToken cancellationToken = default);

    Task RemoveKeyAsync(string key, CancellationToken cancellationToken = default);

    Task RemoveTagAsync(string tag, CancellationToken cancellationToken = default);
}
