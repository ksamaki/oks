using System.Diagnostics;
using System.Reflection;
using Oks.Caching.Abstractions;

namespace Oks.Caching.Internal;

internal static class CacheInvocationContextResolver
{
    public static CacheableAttribute? ResolveCacheable<TMarker>()
        => ResolveFromStack<TMarker, CacheableAttribute>(method =>
            method.GetCustomAttribute<CacheableAttribute>(true)
            ?? method.DeclaringType?.GetCustomAttribute<CacheableAttribute>(true));

    public static CustomCacheAttribute? ResolveCustomCache<TMarker>()
        => ResolveFromStack<TMarker, CustomCacheAttribute>(method =>
            method.GetCustomAttribute<CustomCacheAttribute>(true)
            ?? method.DeclaringType?.GetCustomAttribute<CustomCacheAttribute>(true));

    public static IReadOnlyList<CacheEvictAttribute> ResolveEvictAttributes<TMarker>()
    {
        var result = new List<CacheEvictAttribute>();
        var trace = new StackTrace();

        foreach (var frame in trace.GetFrames() ?? Array.Empty<StackFrame>())
        {
            var method = frame.GetMethod();
            if (method is null)
                continue;

            if (method.DeclaringType?.Assembly == typeof(TMarker).Assembly)
                continue;

            var methodAttributes = method.GetCustomAttributes<CacheEvictAttribute>(true);
            result.AddRange(methodAttributes);

            var classAttributes = method.DeclaringType?.GetCustomAttributes<CacheEvictAttribute>(true)
                ?? Enumerable.Empty<CacheEvictAttribute>();
            result.AddRange(classAttributes);
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
