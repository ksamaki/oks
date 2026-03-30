using Microsoft.Extensions.DependencyInjection;

namespace Oks.Caching.Abstractions;

public sealed class RedisCacheOptions
{
    public bool Enabled { get; set; }

    public string Configuration { get; set; } = string.Empty;

    public string InstanceName { get; set; } = "oks:";

    public Action<IServiceCollection>? ConfigureServices { get; set; }
}
