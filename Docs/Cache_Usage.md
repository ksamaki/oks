# Cache - Usage

[Cache - Description](Cache_Description.md) | [Ana sayfa](../README.md)

## 1) Proje referansları
```xml
<ItemGroup>
  <ProjectReference Include="..\src\Oks\Oks.Caching.Abstractions\Oks.Caching.Abstractions.csproj" />
  <ProjectReference Include="..\src\Oks\Oks.Caching\Oks.Caching.csproj" />
  <ProjectReference Include="..\src\Oks\Oks.Web\Oks.Web.csproj" />
</ItemGroup>
```

## 2) DI kayıtları
```csharp
using Oks.Caching.Extensions;
using Oks.Web.Extensions;

builder.Services.AddOksCaching(options =>
{
    options.UseDistributedCache(services =>
    {
        // örnek: Redis
        services.AddStackExchangeRedisCache(redis =>
        {
            redis.Configuration = builder.Configuration.GetConnectionString("Redis");
            redis.InstanceName = "oks:";
        });
    });

    // hazır kısa yol
    // options.UseRedis(builder.Configuration.GetConnectionString("Redis")!);

    options.AddReadRepositoryCaching();
    options.CacheOnlyRepositoryListQueries(); // varsayılan
});

builder.Services.AddControllers()
    .AddOksCustomCaching();

builder.Services.AddOksCustomCaching(); // Minimal API filter servisi
```

## 3) Entity bazlı otomatik cache
```csharp
using Oks.Caching.Abstractions;

[Cacheable(DurationSeconds = 180, Tags = ["product"])]
public class Product : AuditedEntity<Guid>
{
}
```

Bu durumda `IReadRepository<Product, Guid>` query çağrıları otomatik cache'lenir, `IWriteRepository<Product, Guid>` command çağrılarında ilgili tag/key grupları otomatik temizlenir.

## 4) Action/endpoint bazlı özel cache
```csharp
[CustomCache(DurationSeconds = 60, Tags = ["catalog"])]
[HttpGet("catalog")]
public async Task<IActionResult> GetCatalog() => Ok(await _service.GetCatalogAsync());

[CustomCache(Evict = true, Tags = ["catalog"])]
[HttpPost("catalog/rebuild")]
public async Task<IActionResult> RebuildCatalog() => Ok(await _service.RebuildAsync());
```

## 5) Minimal API ile kullanım
```csharp
var api = app.MapGroup("/api").AddOksCustomCaching();

api.MapGet("/products", [CustomCache(DurationSeconds = 45, Tags = ["product:list"])]
    async (IProductQueryService svc) => await svc.GetAsync());
```

## 6) Elle cache yönetimi
```csharp
public class PriceService(ICacheManager cacheManager)
{
    public Task<decimal> GetPriceAsync(Guid id, CancellationToken ct)
        => cacheManager.GetOrSetAsync($"price:{id}", () => LoadFromSource(id, ct),
            new CacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2),
                Tags = ["price"]
            }, ct);

    public Task ClearAsync(Guid id, CancellationToken ct)
        => cacheManager.RemoveAsync($"price:{id}", ct);
}
```


## 7) DTO -> Entity otomatik etiket eşleme
`[CustomCache]` içinde tag verilmezse framework, action/endpoint argüman ve dönüş tiplerinden
konvansiyonel entity adı üretir (`ProductQuery`, `ProductDto`, `ProductResponse` -> `Product`).
Böylece query/command tarafında dolma/yırtma tagleri otomatik üretilebilir.
