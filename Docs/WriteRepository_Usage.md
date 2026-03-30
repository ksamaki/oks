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
Audit için `OksDbContextBase` kullanılır; kullanıcı bilgisi `IOksUserProvider` üzerinden alınır.

## 3) DI ve pipeline kurulumu
> Not: `Oks.Web` abstraction-only kuralı nedeniyle `AddOksUnitOfWork()` tek başına persistence concrete implementasyonu eklemez. Örnekteki `AddOksEfCore<AppDbContext>()` (veya alternatif persistence kaydı) zorunludur.

```csharp
using Microsoft.EntityFrameworkCore;
using Oks.Persistence.EfCore.Extensions;
using Oks.Web.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddOksEfCore<AppDbContext>();
builder.Services.AddOksCurrentUserProvider();

builder.Services.AddControllers()
    .AddOksUnitOfWork()      // MVC action'larda otomatik commit
    .AddOksResultWrapping(); // MVC result wrapping

var app = builder.Build();

app.UseAuthorization();
app.MapControllers();

var api = app.MapGroup("/api")
    .AddOksUnitOfWork()      // Minimal API commit filter
    .AddOksResultWrapping(); // Minimal API result wrapping filter

api.MapPost("/products", async (Product dto, IWriteRepository<Product, Guid> write) =>
{
    await write.AddAsync(dto);
    return dto;
});

app.Run();
```

## 4) Skip transaction örneği (Minimal API)
```csharp
api.MapPost("/products/import", async (BulkImportRequest request, IWriteRepository<Product, Guid> write) =>
{
    // ... import
    return Result.Ok("İçe aktarma tamamlandı");
})
.WithMetadata(new OksSkipTransactionAttribute());
```

> İpucu: `AddOksUnitOfWork()` aktifken MVC action ve Minimal API endpoint'lerinde başarılı isteklerde commit otomatik denenir. Davranışı endpoint/action bazında kapatmak için `[OksSkipTransaction]` metadata'sını kullanabilirsin.
