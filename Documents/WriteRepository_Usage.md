# Write Repository & Unit of Work - Usage

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
using Oks.Persistence.Abstractions.Read;
using Oks.Persistence.Abstractions.Write;
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

    [HttpDelete("{id}")]
    [OksSkipTransaction] // Gerekirse filtreyi tamamen kapatabilirsin
    public async Task<IActionResult> Delete(Guid id)
    {
        var entity = await _read.GetAsync(id);
        if (entity is null) return NotFound();

        await _write.DeleteAsync(entity);
        await _write.SaveChangesAsync(); // Filtre kapalıyken manuel commit
        return NoContent();
    }
}
```

Bu yapı yazma işlemlerini transaction güvenliği, audit ve soft delete ile birlikte getirir.
