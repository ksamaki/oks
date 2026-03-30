using System.Diagnostics;
using System.Reflection;
using Oks.Caching.Abstractions;

namespace Oks.Caching.Internal;

internal static class CacheInvocationContextResolver
{
    public static OksCacheAttribute? ResolveQueryCache<TMarker>()
        => ResolveFromStack<TMarker, OksCacheAttribute>(method => method.GetCustomAttribute<OksCacheAttribute>(true));

    public static IReadOnlyList<OksCacheInvalidateAttribute> ResolveInvalidationAttributes<TMarker>()
    {
        var result = new List<OksCacheInvalidateAttribute>();
        var trace = new StackTrace();

        foreach (var frame in trace.GetFrames() ?? Array.Empty<StackFrame>())
        {
            var method = frame.GetMethod();
            if (method is null)
                continue;

            if (method.DeclaringType?.Assembly == typeof(TMarker).Assembly)
                continue;

            result.AddRange(method.GetCustomAttributes<OksCacheInvalidateAttribute>(true));
        }

        return result;
    }

    private static TAttribute? ResolveFromStack<TMarker, TAttribute>(Func<MethodBase, TAttribute?> resolver)
        where TAttribute : Attribute
    {
        var trace = new StackTrace();
        foreach (var frame in trace.GetFrames() ?? Array.Empty<StackFrame>())
        {
            var method = frame.GetMethod();
            if (method is null)
                continue;

            if (method.DeclaringType?.Assembly == typeof(TMarker).Assembly)
                continue;

            var attribute = resolver(method);
            if (attribute is not null)
                return attribute;
        }

        return null;
    }
}
