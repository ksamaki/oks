# OKS Framework Kullanım Kılavuzu

Bu doküman, OKS framework bileşenlerini projene nasıl ekleyeceğini ve başlıca yetenekleri nasıl kullanacağını adım adım gösterir. Tüm paketler **opsiyonel**dir; yalnızca ihtiyacın olan modülü eklemen yeterli.

## Temel Bileşenler
- `Oks.Domain`: Base entity tipleri (`Entity`, `AuditedEntity`, `IAuditedEntity`).
- `Oks.Shared`: Ortak sonuç modelleri (`Result`, `DataResult`, `PagedDataResult`).
- `Oks.Persistence.Abstractions` / `Oks.Persistence.EfCore`: Repository, Unit of Work, audit & soft delete altyapısı.
- `Oks.Logging.Abstractions` / `Oks.Logging` / `Oks.Logging.EfCore`: Log yazma kontratı ve EF Core tabanlı log yazıcı + tablolar.
- `Oks.Web`, `Oks.Web.Abstractions`, `Oks.Web.Validation`: ASP.NET Core filtre ve middleware seti (exception, request logging, rate limiting, performance, result wrapping, validation, UoW).

## Hızlı Başlangıç (ASP.NET Core)
1) **Paket/Referans ekle** (örnek `.csproj`):
```xml
<ItemGroup>
  <ProjectReference Include="..\src\Oks\Oks.Domain\Oks.Domain.csproj" />
  <ProjectReference Include="..\src\Oks\Oks.Shared\Oks.Shared.csproj" />
  <ProjectReference Include="..\src\Oks\Oks.Persistence.Abstractions\Oks.Persistence.Abstractions.csproj" />
  <ProjectReference Include="..\src\Oks\Oks.Persistence.EfCore\Oks.Persistence.EfCore.csproj" />
  <ProjectReference Include="..\src\Oks\Oks.Logging.Abstractions\Oks.Logging.Abstractions.csproj" />
  <ProjectReference Include="..\src\Oks\Oks.Logging\Oks.Logging.csproj" />
  <ProjectReference Include="..\src\Oks\Oks.Logging.EfCore\Oks.Logging.EfCore.csproj" />
  <ProjectReference Include="..\src\Oks\Oks.Web.Abstractions\Oks.Web.Abstractions.csproj" />
  <ProjectReference Include="..\src\Oks\Oks.Web\Oks.Web.csproj" />
  <ProjectReference Include="..\src\Oks\Oks.Web.Validation\Oks.Web.Validation.csproj" />
</ItemGroup>
```

2) **DbContext**: `OksDbContextBase`'ten türet, log tablolarını modele ekle.
```csharp
using Microsoft.EntityFrameworkCore;
using Oks.Persistence.EfCore;
using Oks.Logging.EfCore;

public class AppDbContext : OksDbContextBase
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.AddOksLogging(); // OksLog* tabloları
    }

    protected override string? GetCurrentUserIdentifier()
    {
        // Örn: HttpContext User Id veya servis kullanımı
        return "demo-user";
    }
}
```

3) **Program.cs**: Servisleri bağla ve middleware/pipeline'ı ekle.
```csharp
using Oks.Logging.Extensions;
using Oks.Persistence.EfCore;
using Oks.Persistence.EfCore.Extensions;
using Oks.Web.Extensions;
using Oks.Web.Validation;
using Oks.Web.RateLimiting;
using Oks.Web.Performance;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddOksEfCore<AppDbContext>();
builder.Services.AddOksLogging<AppDbContext>();
builder.Services.AddOksRepositoryLogging(opt => opt.Enabled = true);
builder.Services.AddOksAuditLogging(opt => opt.Enabled = true);

builder.Services.AddControllers()
    .AddOksUnitOfWork()
    .AddOksResultWrapping()
    .AddOksRateLimiting(opt => { opt.MaxRequests = 100; opt.WindowSeconds = 60; })
    .AddOksPerformance(opt => { opt.ThresholdMilliseconds = 500; })
    .AddOksFluentValidation(typeof(Program).Assembly);

builder.Services.AddOksExceptionHandling();
builder.Services.AddOksRequestLogging();

var app = builder.Build();

app.UseOksExceptionHandling();
app.UseOksRequestLogging();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

4) **Migration**: DbContext modelinde `AddOksLogging()` olduğu için standart EF migration oluşturman yeterli.
```powershell
# Örnek
Add-Migration InitOksLogs
Update-Database
```

5) **Entity tasarımı**: Soft delete ve audit için `AuditedEntity<TKey>` kullan.
```csharp
using Oks.Domain.Base;

public class Product : AuditedEntity<Guid>
{
    public string Name { get; set; } = default!;
    public decimal Price { get; set; }
}
```

## Özellikler ve Kullanım Örnekleri
### Logging (Request / Exception / Custom)
- `UseOksExceptionHandling()` middleware global hataları yakalar ve `OksLogException` tablosuna yazar.
- `UseOksRequestLogging()` tüm HTTP isteklerini süre ve durum koduyla `OksLogRequest` tablosuna loglar.
- Özel log yazmak için `IOksLogWriter`'ı enjekte et:
```csharp
using Oks.Logging.Abstractions.Interfaces;
using Oks.Logging.Abstractions.Models;
using Oks.Logging.Abstractions.Enums;
using Oks.Logging.Abstractions.Extensions;

public class DemoService
{
    private readonly IOksLogWriter _logWriter;
    public DemoService(IOksLogWriter logWriter) => _logWriter = logWriter;

    public async Task DoWorkAsync()
    {
        await _logWriter.SafeWriteAsync(new OksLogEntry
        {
            Category = OksLogCategory.Custom,
            Level = OksLogLevel.Info,
            Message = "Custom log ornegi",
            CreatedAtUtc = DateTime.UtcNow,
            ExtraDataJson = "{\"key\":\"value\"}"
        });
    }
}
```

### Repository + Unit of Work
- DI'dan `IReadRepository<TEntity, TKey>` ve `IWriteRepository<TEntity, TKey>` kullan.
- `OksUnitOfWorkFilter` yazma yapıldığında action sonunda `SaveChangesAsync` çağırır.
- `[OksTransactional]` ile yazma olmasa bile commit zorlanır, `[OksSkipTransaction]` ile tamamen devre dışı bırakılır.
```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IReadRepository<Product, Guid> _read;
    private readonly IWriteRepository<Product, Guid> _write;

    public ProductsController(IReadRepository<Product, Guid> read, IWriteRepository<Product, Guid> write)
    {
        _read = read; _write = write;
    }

    [HttpGet]
    public Task<List<Product>> Get() => _read.GetListAsync();

    [HttpPost]
    [OksTransactional]
    public async Task<IActionResult> Create(Product dto)
    {
        await _write.AddAsync(dto);
        return Ok(dto); // SaveChanges action sonunda otomatik tetiklenir
    }
}
```

### Validation (FluentValidation)
- `AddOksFluentValidation()` validator'ları DI'a ekler, `OksValidationFilter` otomatik çalışır.
- `[OksSkipValidation]` ile kapatılabilir.
```csharp
public class CreateProductValidator : AbstractValidator<Product>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Price).GreaterThan(0);
    }
}
```

### Rate Limiting & Performance
- `AddOksRateLimiting` ve `AddOksPerformance` ile global filtre eklenir.
- `[OksSkipRateLimit]`, `[OksRateLimit]`, `[OksSkipPerformance]`, `[OksPerformance]` attribute'larıyla özelleştirilebilir.
```csharp
[OksRateLimit(MaxRequests = 10, WindowSeconds = 60)]
[OksPerformance(ThresholdMilliseconds = 200)]
public async Task<IActionResult> HeavyWork() { ... }
```

### Audit & Soft Delete
- `AuditedEntity` ve `OksDbContextBase` birlikte çalışır: `SaveChangesAsync` çağrısından önce audit alanları otomatik dolar, silme operasyonları soft delete'e çevrilir.
- Global query filter `IsDeleted = false` uygular; gerekirse `IsAuditEnabled = false` diyerek entity bazında kapatabilirsin.
- `EfUnitOfWork` audit loglarını `OksLogAudit` tablosuna yazar; `AddOksAuditLogging(opt => opt.Enabled = true)` ile açılır.

### Repository Logging
- `AddOksRepositoryLogging(opt => opt.Enabled = true)` çağırıldığında repository okuma/yazma süreleri `OksLogRepository` tablosuna düşer.

### İpucu: Correlation ve Kullanıcı
- `GetCurrentUserIdentifier()` override edilerek audit ve loglar için user id sağlanabilir.
- `HttpContext.TraceIdentifier` otomatik correlation id olarak kullanılır; istersen `OksLogEntry.CorrelationId` ile özel değer verebilirsin.

## Hızlı Kontrol Listesi
- DbContext: `OksDbContextBase` + `modelBuilder.AddOksLogging()`
- Services: `AddOksEfCore`, `AddOksLogging<TDbContext>`, ihtiyaca göre `AddOksAuditLogging` ve `AddOksRepositoryLogging`
- MVC: `AddOksUnitOfWork`, `AddOksResultWrapping`, opsiyonel `AddOksRateLimiting`, `AddOksPerformance`, `AddOksFluentValidation`
- Middleware: `UseOksExceptionHandling`, `UseOksRequestLogging`
- Entity: `AuditedEntity<TKey>` türet, gerekiyorsa `IsAuditEnabled` ayarla
