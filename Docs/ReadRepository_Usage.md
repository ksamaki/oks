# Read-only Repository - Usage

[Read-only Repository - Description](ReadRepository_Description.md) | [Ana sayfa](../README.md)

Aşağıdaki adımları doğrudan kopyala-yapıştır ederek projene read-only repository katmanını ekleyebilirsin.

## 1) Proje referanslarını ekle (`.csproj`)
```xml
<ItemGroup>
  <ProjectReference Include="..\\src\\Oks\\Oks.Domain\\Oks.Domain.csproj" />
  <ProjectReference Include="..\\src\\Oks\\Oks.Persistence.Abstractions\\Oks.Persistence.Abstractions.csproj" />
  <ProjectReference Include="..\\src\\Oks\\Oks.Persistence.EfCore\\Oks.Persistence.EfCore.csproj" />
  <ProjectReference Include="..\\src\\Oks\\Oks.Web\\Oks.Web.csproj" />
</ItemGroup>
```

## 2) DbContext'i hazırla
`OksDbContextBase` soft delete ve audit alanlarını yönetir.
```csharp
using Microsoft.EntityFrameworkCore;
using Oks.Persistence.EfCore;

public class AppDbContext : OksDbContextBase
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
}
```

## 3) DI kaydı
```csharp
using Microsoft.EntityFrameworkCore;
using Oks.Persistence.EfCore;
using Oks.Web.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddOksEfCore<AppDbContext>();
builder.Services.AddOksCurrentUserProvider(); // Web claim tabanlı IOksUserProvider
// Minimal API endpoint'lerinde de aynı provider geçerlidir.
```

## 4) Controller örneği
```csharp
using Microsoft.AspNetCore.Mvc;
using Oks.Persistence.Abstractions.Repositories;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IReadRepository<Product, Guid> _read;

    public ProductsController(IReadRepository<Product, Guid> read)
    {
        _read = read;
    }

    [HttpGet]
    public Task<List<Product>> Get() => _read.GetListAsync();

    [HttpGet("search")]
    public Task<Product?> GetBySku(string sku) => _read.GetAsync(x => x.Sku == sku);

    [HttpGet("{id:guid}")]
    public Task<Product?> GetById(Guid id) => _read.GetByIdAsync(id);
}
```

## 5) SQL seviyesinde filtre + paging/sorting
`GetAsync` ve `GetListAsync(predicate)` parametreleri `Expression<Func<...>>` aldığı için EF Core tarafından SQL'e çevrilir.

Daha ileri sorting/paging için `Query()` üzerinden genişleyebilirsin:
```csharp
var page = await _read.Query()
    .Where(x => x.CategoryId == categoryId)
    .OrderBy(x => x.Name)
    .Skip((pageIndex - 1) * pageSize)
    .Take(pageSize)
    .ToListAsync(cancellationToken);
```
