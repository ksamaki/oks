using System;

namespace Oks.Domain.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class EnableRepositoryCacheAttribute : Attribute
{
    public EnableRepositoryCacheAttribute(int absoluteExpirationSeconds = 300)
    {
        AbsoluteExpirationSeconds = absoluteExpirationSeconds;
    }

    /// <summary>
    /// Absolute expiration in seconds for the cached item.
    /// </summary>
    public int AbsoluteExpirationSeconds { get; }

    /// <summary>
    /// Optional sliding expiration in seconds. When null, no sliding expiration is used.
    /// </summary>
    public int? SlidingExpirationSeconds { get; set; }

    public TimeSpan AbsoluteExpiration => TimeSpan.FromSeconds(AbsoluteExpirationSeconds);

    public TimeSpan? SlidingExpiration =>
        SlidingExpirationSeconds is null
            ? null
            : TimeSpan.FromSeconds(SlidingExpirationSeconds.Value);
}
