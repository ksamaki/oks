# Cache - Usage

[Cache - Description](Cache_Description.md) | [Ana sayfa](../README.md)

Cache özelliğini projene eklemek için aşağıdaki adımları uygulayabilirsin.

## 1) Proje referansları (`.csproj`)
```xml
<ItemGroup>
  <ProjectReference Include="..\src\Oks\Oks.Domain\Oks.Domain.csproj" />
  <ProjectReference Include="..\src\Oks\Oks.Shared\Oks.Shared.csproj" />
  <ProjectReference Include="..\src\Oks\Oks.Persistence.EfCore\Oks.Persistence.EfCore.csproj" />
  <ProjectReference Include="..\src\Oks\Oks.Caching.Abstractions\Oks.Caching.Abstractions.csproj" />
  <ProjectReference Include="..\src\Oks\Oks.Caching\Oks.Caching.csproj" />
  <ProjectReference Include="..\src\Oks\Oks.Caching.Redis\Oks.Caching.Redis.csproj" /> <!-- Redis opsiyonel -->
  <ProjectReference Include="..\src\Oks\Oks.Web.Abstractions\Oks.Web.Abstractions.csproj" />
  <ProjectReference Include="..\src\Oks\Oks.Web\Oks.Web.csproj" />
</ItemGroup>
```

## 2) DI ve cache servislerini kaydet
```csharp
using Oks.Caching.Extensions;
using Oks.Persistence.EfCore.Extensions;
using Oks.Web.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddOksEfCore<AppDbContext>();

builder.Services.AddOksCaching(caching =>
{
    caching.UseDistributedCache(); // IMemoryCache yerine Redis seçmek için
    caching.WithDefaultOptions(o =>
    {
        o.Duration = TimeSpan.FromMinutes(5);
        o.SoftTtl = TimeSpan.FromSeconds(30);
        o.Tags = new Dictionary<string, string[]>
        {
            ["User"] = new[] { "Query:User" },
            ["Order"] = new[] { "Query:Order" }
        };
    });
    caching.AddReadRepositoryCaching();
    caching.AddWriteRepositoryEviction();
    caching.AddMvcFilters(); // Cacheable/CacheEvict filterları
});

builder.Services.AddControllers()
    .AddOksResultWrapping();

var app = builder.Build();

app.UseAuthorization();
app.MapControllers();

app.Run();
```

## 3) MVC veya Minimal API'de attribute kullanımı
```csharp
using Microsoft.AspNetCore.Mvc;
using Oks.Caching.Abstractions.Attributes;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    [HttpGet("{id}")]
    [Cacheable(KeyTemplate = "users/{id}", DurationSeconds = 300, Tags = new[] { "User", "User:{id}" })]
    public async Task<IActionResult> GetById([FromRoute] Guid id, [FromServices] IUserReadRepository repo)
    {
        var user = await repo.GetAsync(id);
        return user is null ? NotFound() : Ok(user);
    }

    [HttpGet]
    [Cacheable(KeyTemplate = "users?page={page}&size={size}", Tags = new[] { "Query:User" })]
    public async Task<IActionResult> GetPaged([FromQuery] int page, [FromQuery] int size, [FromServices] IUserReadRepository repo)
    {
        var users = await repo.PagedAsync(page, size);
        return Ok(users);
    }

    [HttpPost]
    [CacheEvict(Tags = new[] { "User" }, EvictAllEntityCache = true)]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request, [FromServices] IUserWriteRepository repo)
    {
        var id = await repo.InsertAsync(request.ToEntity());
        return CreatedAtAction(nameof(GetById), new { id }, null);
    }
}
```

### Attribute ipuçları
- `KeyTemplate` path + query parametreleri deterministik şekilde birleştirir; tenant veya kullanıcı kimliği için `{tenantId}` gibi özel segmentler ekleyebilirsin.
- `[CacheEvict]` içinde `EvictAllEntityCache = true` ayarı, `EntityName`, `EntityName:{id}` ve `Query:EntityName` tag'lerini topluca temizlemek için kullanılır.
- Minimal API veya MediatR handler'larında attribute desteği için ilgili filter/behavior eklentilerini açmak yeterlidir.

## 4) Repository dekoratörleri ile otomatik cache
```csharp
using Microsoft.Extensions.DependencyInjection;
using Oks.Caching.Extensions;
using Oks.Persistence.EfCore.Extensions;

var services = new ServiceCollection();

services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer("connection-string"));

services.AddOksEfCore<AppDbContext>();
services.AddOksCaching(caching =>
{
    caching.AddReadRepositoryCaching(); // IReadRepository çağrıları cache'lenir
    caching.AddWriteRepositoryEviction(); // Insert/Update/Delete sonrası ilgili tag'ler temizlenir
});
```

`ReadRepositoryCacheDecorator` aşağıdaki deterministik key formatını kullanır:
- `EntityName:Method:IdsHash:FiltersHash:Page:Size:Includes`
- Liste veya sayfalama çağrılarında parametreler sıralanır ve SHA256 ile hash'lenir.

## 5) Yırtma kuralları ve tag adlandırma
- Tekil entity: `User:{id}`
- Tüm entity: `User`
- Liste/Sorgu: `Query:User`, `Query:User:Active`
- Custom senaryo: domain event ile `CacheInvalidationHandler` üzerinden `RemoveByTagAsync("User:{id}")`

## 6) Redis provider'ını etkinleştirme
```csharp
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "oks:";
});

builder.Services.AddOksCaching(caching =>
{
    caching.UseDistributedCache(); // Redis için IDistributedCache kullan
    caching.AddReadRepositoryCaching();
});
```

## 7) Soft-TTL ve single-flight ayarı
```csharp
builder.Services.AddOksCaching(caching =>
{
    caching.WithDefaultOptions(o =>
    {
        o.Duration = TimeSpan.FromMinutes(3);
        o.SoftTtl = TimeSpan.FromSeconds(20); // süre bitimine yakın kısa uzatma
        o.SingleFlightTimeout = TimeSpan.FromSeconds(5); // aynı key için tek üretici
    });
});
```

## 8) Telemetri ve loglama hook'ları
`ICacheService` olaylarını dinlemek için `IOksLogWriter` implementasyonu ekleyebilir veya `Meter` ile metrik üretebilirsin. Hit/miss, set/remove ve yırtma olayları hook noktalarından geçer.

Bu adımların ardından `[Cacheable]` ile cache'e alma, `[CacheEvict]` ile yırtma ve repository dekoratörleriyle otomatik tag yönetimi çalışır. Soft-TTL, deterministik cache key'ler ve hit/miss telemetrisi varsayılan davranış olarak gelir.
