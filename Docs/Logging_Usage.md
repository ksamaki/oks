# Logging - Usage

[Logging - Description](Logging_Description.md) | [Ana sayfa](../README.md)

Aşağıdaki örnek, tüm log kategorilerini MVC + Minimal API üzerinde birlikte gösterir.

## 1) Proje referansları
```xml
<ItemGroup>
  <ProjectReference Include="..\\src\\Oks\\Oks.Persistence.EfCore\\Oks.Persistence.EfCore.csproj" />
  <ProjectReference Include="..\\src\\Oks\\Oks.Logging.Abstractions\\Oks.Logging.Abstractions.csproj" />
  <ProjectReference Include="..\\src\\Oks\\Oks.Logging\\Oks.Logging.csproj" />
  <ProjectReference Include="..\\src\\Oks\\Oks.Logging.EfCore\\Oks.Logging.EfCore.csproj" />
  <ProjectReference Include="..\\src\\Oks\\Oks.Web.Abstractions\\Oks.Web.Abstractions.csproj" />
  <ProjectReference Include="..\\src\\Oks\\Oks.Web\\Oks.Web.csproj" />
</ItemGroup>
```

> Not (2026-03-30): `Oks.Web` abstraction-only olduğu için web katmanı `Oks.Logging` concrete paketini transitif olarak getirmez. Bu usage'daki `AddOksLogging<AppDbContext>()` (veya eşdeğer kendi `IOksLogWriter` kaydın) zorunludur.

## 2) DbContext
```csharp
using Microsoft.EntityFrameworkCore;
using Oks.Logging.EfCore;
using Oks.Persistence.EfCore;

public class AppDbContext : OksDbContextBase
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.AddOksLogging();
    }

    }
```

## 3) DI + Middleware + MVC/Minimal API pipeline
```csharp
using Microsoft.EntityFrameworkCore;
using Oks.Logging.Extensions;
using Oks.Persistence.EfCore.Extensions;
using Oks.Web.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddOksEfCore<AppDbContext>();
builder.Services.AddOksCurrentUserProvider();

// Writer + repository/audit logging
builder.Services.AddOksLogging<AppDbContext>();
builder.Services.AddOksRepositoryLogging(opt => opt.Enabled = true);
builder.Services.AddOksAuditLogging(opt => opt.Enabled = true);

// Request + Exception logging middleware
builder.Services.AddOksRequestLogging();
builder.Services.AddOksExceptionHandling();

// Performance + RateLimit options (MVC ve Minimal API için)
builder.Services.AddOksPerformance(opt =>
{
    opt.Enabled = true;
    opt.DefaultThresholdMs = 500;
    opt.ThrowOnSlowRequest = false;
});

builder.Services.AddOksRateLimiting(opt =>
{
    opt.Enabled = true;
    opt.DefaultRequestsPerMinute = 100;
});

builder.Services.AddControllers()
    .AddOksResultWrapping()
    .AddOksPerformance()
    .AddOksRateLimiting();

var app = builder.Build();

app.UseOksExceptionHandling();
app.UseOksRequestLogging();

app.MapControllers();

var api = app.MapGroup("/api")
    .AddOksResultWrapping()
    .AddOksPerformance()
    .AddOksRateLimiting();

app.Run();
```

## 4) Minimal API örnekleri (Performance + RateLimit metadata)
```csharp
using Oks.Web.Abstractions.Attributes;

api.MapGet("/slow", async () =>
{
    await Task.Delay(800);
    return "slow endpoint";
})
.WithMetadata(new OksPerformanceAttribute(200));

api.MapGet("/limited", () => "limited")
   .WithMetadata(new OksRateLimitAttribute(10));

api.MapGet("/limited-skip", () => "skip")
   .WithMetadata(new OksRateLimitAttribute(1))
   .WithMetadata(new OksSkipRateLimitAttribute());
```

## 5) MVC örnekleri (Performance + RateLimit)
```csharp
[ApiController]
[Route("api/[controller]")]
public class DemoController : ControllerBase
{
    [HttpGet("fast")]
    [OksPerformance(200)]
    public IActionResult Fast() => Ok("ok");

    [HttpGet("limited")]
    [OksRateLimit(10)]
    public IActionResult Limited() => Ok("rate-limited");

    [HttpGet("skip")]
    [OksSkipPerformance]
    [OksSkipRateLimit]
    public IActionResult Skip() => Ok("skip");
}
```

## 6) Repository + Audit log örneği
```csharp
public sealed class ProductService
{
    private readonly IWriteRepository<Product, Guid> _write;

    public ProductService(IWriteRepository<Product, Guid> write)
    {
        _write = write;
    }

    public async Task CreateAsync(Product product)
    {
        await _write.AddAsync(product);
        // SaveChanges AddOksUnitOfWork ile otomatik veya manuel çağrılabilir.
        // Repository/Audit logları burada devreye girer.
    }
}
```

## 7) Custom log örneği
```csharp
using Oks.Logging.Abstractions.Enums;
using Oks.Logging.Abstractions.Extensions;
using Oks.Logging.Abstractions.Interfaces;
using Oks.Logging.Abstractions.Models;

public sealed class PaymentService
{
    private readonly IOksLogWriter _logWriter;

    public PaymentService(IOksLogWriter logWriter) => _logWriter = logWriter;

    public async Task MarkAsync(Guid orderId)
    {
        await _logWriter.SafeWriteAsync(new OksLogEntry
        {
            Category = OksLogCategory.Custom,
            Level = OksLogLevel.Info,
            Message = $"Payment marked: {orderId}",
            CreatedAtUtc = DateTime.UtcNow
        });
    }
}
```
