# Write Repository & Unit of Work - Usage

[Write Repository & Unit of Work - Description](WriteRepository_Description.md) | [Ana sayfa](../README.md)

Aşağıdaki adımları kopyalayarak yazma repository katmanını transaction filtreleriyle birlikte devreye alabilirsin.

## 1) Proje referanslarını ekle (`.csproj`)
```xml
<ItemGroup>
  <ProjectReference Include="..\\src\\Oks\\Oks.Domain\\Oks.Domain.csproj" />
  <ProjectReference Include="..\\src\\Oks\\Oks.Shared\\Oks.Shared.csproj" />
  <ProjectReference Include="..\\src\\Oks\\Oks.Persistence.Abstractions\\Oks.Persistence.Abstractions.csproj" />
  <ProjectReference Include="..\\src\\Oks\\Oks.Persistence.EfCore\\Oks.Persistence.EfCore.csproj" />
  <ProjectReference Include="..\\src\\Oks\\Oks.Web.Abstractions\\Oks.Web.Abstractions.csproj" />
  <ProjectReference Include="..\\src\\Oks\\Oks.Web\\Oks.Web.csproj" />
</ItemGroup>
```

## 2) DbContext ve audit
Audit için `OksDbContextBase` kullanılır; kullanıcı bilgisini audit kayıtlarında kullanmak için `GetCurrentUserIdentifier`'ı doldur.
```csharp
using Microsoft.EntityFrameworkCore;
using Oks.Persistence.EfCore;
using Oks.Logging.EfCore; // Audit log tablolarını aktifleştirmek istersen

public class AppDbContext : OksDbContextBase
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.AddOksLogging(); // OksLog* tablolarını eklemek opsiyoneldir
    }

    protected override string? GetCurrentUserIdentifier()
    {
        return "demo-user"; // HttpContext'ten veya servislerinden alabilirsin
    }
}
```

## 3) DI ve pipeline kurulumu
```csharp
using Microsoft.EntityFrameworkCore;
using Oks.Persistence.EfCore.Extensions;
using Oks.Web.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddOksEfCore<AppDbContext>();

builder.Services.AddControllers()
    .AddOksUnitOfWork()    // Action sonunda otomatik SaveChanges
    .AddOksResultWrapping();

var app = builder.Build();

app.UseAuthorization();
app.MapControllers();

app.Run();
```

## 4) Yazma işlemi yapan controller örneği
```csharp
using Microsoft.AspNetCore.Mvc;
using Oks.Persistence.Abstractions.Repositories;
using Oks.Web.Attributes;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IReadRepository<Product, Guid> _read;
    private readonly IWriteRepository<Product, Guid> _write;

    public ProductsController(IReadRepository<Product, Guid> read, IWriteRepository<Product, Guid> write)
    {
        _read = read;
        _write = write;
    }

    [HttpPost]
    [OksTransactional] // Yazma olmasa bile transaction/commit zorlanır
    public async Task<IActionResult> Create(Product dto)
    {
        await _write.AddAsync(dto);
        return Ok(dto); // SaveChanges action sonunda otomatik tetiklenir
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, Product dto)
    {
        var entity = await _read.GetByIdAsync(id);
        if (entity is null) return NotFound();

        entity.Name = dto.Name;
        entity.Price = dto.Price;

        await _write.UpdateAsync(entity);
        return Ok(entity); // SaveChanges action sonunda otomatik tetiklenir
    }

    [HttpDelete("{id}")]
    [OksSkipTransaction] // Gerekirse filtreyi tamamen kapatabilirsin
    public async Task<IActionResult> Delete(Guid id)
    {
        var entity = await _read.GetByIdAsync(id);
        if (entity is null) return NotFound();

        await _write.RemoveAsync(entity);
        // Filtre kapalıyken manuel commit için IUnitOfWork enjekte edip SaveChangesAsync çağır.
        return NoContent();
    }
}
```

> İpucu: `[OksTransactional]` attribute'u sınıf seviyesine koyarak tüm action'larda commit'i zorunlu kılabilir, `[OksSkipTransaction]` ile belirli controller'larda unit of work filtresini tamamen devre dışı bırakabilirsin. `UpdateAsync` ve `RemoveAsync`, EF Core tarafında izleme durumunu değiştiren senkron işlemleri sarmalayarak async API bütünlüğü sağlar.

Bu yapı yazma işlemlerini transaction güvenliği, audit ve soft delete ile birlikte getirir.
