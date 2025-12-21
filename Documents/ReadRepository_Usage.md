# Read-only Repository - Usage

[Read-only Repository - Description](ReadRepository_Description.md) | [Ana sayfa](../README.md)

Aşağıdaki adımları doğrudan kopyala-yapıştır ederek projene read-only repository katmanını ekleyebilirsin.

## 1) Proje referanslarını ekle (`.csproj`)
```xml
<ItemGroup>
  <ProjectReference Include="..\\src\\Oks\\Oks.Domain\\Oks.Domain.csproj" />
  <ProjectReference Include="..\\src\\Oks\\Oks.Shared\\Oks.Shared.csproj" />
  <ProjectReference Include="..\\src\\Oks\\Oks.Persistence.Abstractions\\Oks.Persistence.Abstractions.csproj" />
  <ProjectReference Include="..\\src\\Oks\\Oks.Persistence.EfCore\\Oks.Persistence.EfCore.csproj" />
  <ProjectReference Include="..\\src\\Oks\\Oks.Web.Abstractions\\Oks.Web.Abstractions.csproj" />
</ItemGroup>
```

## 2) DbContext'i hazırla
`OksDbContextBase` soft delete ve audit alanlarını yönetir; read-only senaryoda da aynı taban sınıfı kullanılabilir.
```csharp
using Microsoft.EntityFrameworkCore;
using Oks.Persistence.EfCore;

public class AppDbContext : OksDbContextBase
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override string? GetCurrentUserIdentifier()
    {
        // Okuma senaryosunda kullanıcıyı izlemek zorunlu değildir.
        return null;
    }
}
```

## 3) DI kaydı ve minimal pipeline
```csharp
using Microsoft.EntityFrameworkCore;
using Oks.Persistence.EfCore.Extensions;
using Oks.Web.Extensions; // Result wrapping için

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddOksEfCore<AppDbContext>(); // IReadRepository / IWriteRepository kayıtları

builder.Services.AddControllers()
    .AddOksResultWrapping(); // Opsiyonel: standart API yanıtları

var app = builder.Build();

app.UseAuthorization();
app.MapControllers();

app.Run();
```

## 4) Okuma yapan bir controller örneği
```csharp
using Microsoft.AspNetCore.Mvc;
using Oks.Persistence.Abstractions.Read;

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

    [HttpGet("{id}")]
    public Task<Product?> GetById(Guid id) => _read.GetAsync(id);
}
```

Bu yapı sayesinde yazma yetkileri olmadan, otomatik soft delete filtresiyle güvenli okuma yapabilirsin.
