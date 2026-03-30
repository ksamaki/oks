using System.Reflection;

namespace Oks.Caching.Abstractions;

public interface ICacheKeyGenerator
{
    string GenerateKey(string template, MethodInfo method, IReadOnlyDictionary<string, object?> arguments);

    IReadOnlyCollection<string> GenerateTags(IEnumerable<string> templates, MethodInfo method, IReadOnlyDictionary<string, object?> arguments);
}
