namespace Oks.Caching.Abstractions;

public interface ICacheEntityNameResolver
{
    IReadOnlyCollection<string> ResolveFromType(Type type);
}
