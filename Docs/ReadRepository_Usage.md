# Read-only Repository - Usage

[Read-only Repository - Description](ReadRepository_Description.md) | [Ana sayfa](../README.md)

Asagidaki adimlari dogrudan kopyala-yapistir ederek projene read-only repository katmanini ekleyebilirsin.

## 1) Proje referanslarini ekle (`.csproj`)

```xml
<ItemGroup>
  <ProjectReference Include="..\\src\\Oks\\Oks.Domain\\Oks.Domain.csproj" />
  <ProjectReference Include="..\\src\\Oks\\Oks.Persistence.Abstractions\\Oks.Persistence.Abstractions.csproj" />
  <ProjectReference Include="..\\src\\Oks\\Oks.Persistence.EfCore\\Oks.Persistence.EfCore.csproj" />
  <ProjectReference Include="..\\src\\Oks\\Oks.Web\\Oks.Web.csproj" />
</ItemGroup>
```

## 2) DbContext'i hazirla

```csharp
using Microsoft.EntityFrameworkCore;
using Oks.Persistence.EfCore;

public class AppDbContext : OksDbContextBase
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
}
```

## 3) DI kaydi

```csharp
using Microsoft.EntityFrameworkCore;
using Oks.Persistence.EfCore;
using Oks.Web.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddOksEfCore<AppDbContext>();
builder.Services.AddOksCurrentUserProvider();
```

## 4) Controller ornegi

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

    [HttpGet("{id:guid}")]
    public Task<Product?> GetById(Guid id) =>
        _read.GetByIdAsync(id);

    [HttpGet("search")]
    public Task<Product?> GetBySku(string sku, CancellationToken cancellationToken) =>
        _read.Query(x => x.Sku == sku)
            .FirstOrDefaultAsync(cancellationToken);

    [HttpGet("active")]
    public Task<List<Product>> GetActive(CancellationToken cancellationToken) =>
        _read.GetListAsync(x => x.IsActive, cancellationToken);

    [HttpGet("exists")]
    public Task<bool> Exists(string sku, CancellationToken cancellationToken) =>
        _read.Query(x => x.Sku == sku)
            .AnyAsync(cancellationToken);

    [HttpGet("count")]
    public Task<int> CountActive(CancellationToken cancellationToken) =>
        _read.Query(x => x.IsActive)
            .CountAsync(cancellationToken);
}
```

## 5) Dogru kullanim

Tek kayit ama first-match yeterliyse:

```csharp
var friendship = await _read.Query(
        x => x.Id == requestId && x.FriendUserId == receiverId)
    .FirstOrDefaultAsync(cancellationToken);
```

Tek kayit ve uniqueness bekleniyorsa:

```csharp
var friendship = await _read.Query(
        x => x.Id == requestId && x.FriendUserId == receiverId)
    .SingleOrDefaultAsync(cancellationToken);
```

Liste ararken:

```csharp
var activeUsers = await _read.GetListAsync(
    x => x.IsActive,
    cancellationToken);
```

Sorting/paging yaparken:

```csharp
var page = await _read.Query(x => x.CategoryId == categoryId)
    .OrderBy(x => x.Name)
    .Skip((pageIndex - 1) * pageSize)
    .Take(pageSize)
    .ToListAsync(cancellationToken);
```

## 6) Anti-pattern

Yanlis:

```csharp
var all = await _read.GetListAsync(cancellationToken: cancellationToken);
var friendship = all.FirstOrDefault(x => x.Id == requestId);
```

Yanlis:

```csharp
var all = await _read.GetListAsync();
var exists = all.Any(x => x.Sku == sku);
```

Dogru:

```csharp
var friendship = await _read.Query(x => x.Id == requestId)
    .FirstOrDefaultAsync(cancellationToken);
```

Dogru:

```csharp
var exists = await _read.Query(x => x.Sku == sku)
    .AnyAsync(cancellationToken);
```

## 7) Code Review kurali

Asagidaki kullanimlar review'de smell kabul edilmelidir:

- `GetListAsync()` sonrasi `FirstOrDefault`
- `GetListAsync()` sonrasi `Where`
- `GetListAsync()` sonrasi `SingleOrDefault`
- `GetListAsync()` sonrasi `Any`
- `GetListAsync()` sonrasi `Count`

Buradaki hedef framework API'sini zorla daraltmak degil, yazilimciyi `IQueryable + LINQ async` standardina yonlendirmektir.
