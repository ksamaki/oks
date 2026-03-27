using System.Text.RegularExpressions;
using Oks.Caching.Abstractions;

namespace Oks.Caching;

public sealed class ConventionCacheEntityNameResolver : ICacheEntityNameResolver
{
    private static readonly string[] Suffixes =
    [
        "Dto", "Request", "Response", "Model", "Vm", "ViewModel", "Query", "Command", "Result"
    ];

    public IReadOnlyCollection<string> ResolveFromType(Type type)
    {
        if (type == typeof(string))
            return Array.Empty<string>();

        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var normalized = Normalize(type.Name);
        if (!string.IsNullOrWhiteSpace(normalized))
            names.Add(normalized);

        if (type.IsGenericType)
        {
            foreach (var arg in type.GetGenericArguments())
            {
                foreach (var nested in ResolveFromType(arg))
                    names.Add(nested);
            }
        }

        return names.ToArray();
    }

    private static string Normalize(string raw)
    {
        var candidate = raw;

        foreach (var suffix in Suffixes)
        {
            if (candidate.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
            {
                candidate = candidate[..^suffix.Length];
                break;
            }
        }

        candidate = Regex.Replace(candidate, "[^A-Za-z0-9]", string.Empty);
        return candidate;
    }
}
