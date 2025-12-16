using System;

namespace Oks.Persistence.EfCore.Options;

public sealed class OksRepositoryCacheOptions
{
    /// <summary>
    /// Turns on caching for repositories. When false, repositories work as before.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Uses <see cref="Oks.Domain.Attributes.EnableRepositoryCacheAttribute"/> on the entity
    /// to decide whether caching is active. When false, all entities are cached.
    /// </summary>
    public bool RespectEntityAttribute { get; set; } = true;

    /// <summary>
    /// Default absolute expiration for cache entries when the attribute does not override it.
    /// </summary>
    public TimeSpan DefaultAbsoluteExpiration { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Default sliding expiration when the attribute does not override it.
    /// </summary>
    public TimeSpan? DefaultSlidingExpiration { get; set; }
}
