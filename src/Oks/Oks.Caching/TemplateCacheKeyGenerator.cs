using System.Reflection;
using System.Text.RegularExpressions;
using Oks.Caching.Abstractions;

namespace Oks.Caching;

public sealed partial class TemplateCacheKeyGenerator : ICacheKeyGenerator
{
    [GeneratedRegex("\\{(?<name>[a-zA-Z0-9_]+)\\}")]
    private static partial Regex PlaceholderRegex();

    public string GenerateKey(string template, MethodInfo method, IReadOnlyDictionary<string, object?> arguments)
        => ResolveTemplate(template, method, arguments);

    public IReadOnlyCollection<string> GenerateTags(IEnumerable<string> templates, MethodInfo method, IReadOnlyDictionary<string, object?> arguments)
        => templates.Select(template => ResolveTemplate(template, method, arguments)).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();

    private static string ResolveTemplate(string template, MethodInfo method, IReadOnlyDictionary<string, object?> arguments)
    {
        if (string.IsNullOrWhiteSpace(template))
            throw new InvalidOperationException($"Cache template cannot be empty on {method.DeclaringType?.FullName}.{method.Name}.");

        return PlaceholderRegex().Replace(template, match =>
        {
            var name = match.Groups["name"].Value;
            if (!arguments.TryGetValue(name, out var value) || value is null)
                return "null";

            return value is IFormattable f
                ? f.ToString(null, System.Globalization.CultureInfo.InvariantCulture)
                : value.ToString() ?? "null";
        });
    }
}
