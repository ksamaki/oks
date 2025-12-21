# Logging - Usage

Projene OKS logging'i eklemek için aşağıdaki adımları kopyalayabilirsin.

## 1) Proje referansları (`.csproj`)
```xml
<ItemGroup>
  <ProjectReference Include="..\\src\\Oks\\Oks.Domain\\Oks.Domain.csproj" />
  <ProjectReference Include="..\\src\\Oks\\Oks.Shared\\Oks.Shared.csproj" />
  <ProjectReference Include="..\\src\\Oks\\Oks.Persistence.EfCore\\Oks.Persistence.EfCore.csproj" />
  <ProjectReference Include="..\\src\\Oks\\Oks.Logging.Abstractions\\Oks.Logging.Abstractions.csproj" />
  <ProjectReference Include="..\\src\\Oks\\Oks.Logging\\Oks.Logging.csproj" />
  <ProjectReference Include="..\\src\\Oks\\Oks.Logging.EfCore\\Oks.Logging.EfCore.csproj" />
  <ProjectReference Include="..\\src\\Oks\\Oks.Web.Abstractions\\Oks.Web.Abstractions.csproj" />
  <ProjectReference Include="..\\src\\Oks\\Oks.Web\\Oks.Web.csproj" />
</ItemGroup>
```

## 2) DbContext'e log tablolarını ekle
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
        modelBuilder.AddOksLogging(); // OksLog* tablolarını EF modeline ekle
    }

    protected override string? GetCurrentUserIdentifier()
    {
        return "demo-user"; // Audit ve loglar için kullanıcı kimliği
    }
}
```

## 3) DI ve middleware kaydı
```csharp
using Microsoft.EntityFrameworkCore;
using Oks.Logging.Extensions;
using Oks.Persistence.EfCore.Extensions;
using Oks.Web.Extensions;
using Oks.Web.Performance;
using Oks.Web.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddOksEfCore<AppDbContext>();
builder.Services.AddOksLogging<AppDbContext>();
builder.Services.AddOksRepositoryLogging(opt => opt.Enabled = true);
builder.Services.AddOksAuditLogging(opt => opt.Enabled = true);

builder.Services.AddControllers()
    .AddOksResultWrapping()
    .AddOksRateLimiting(opt => { opt.MaxRequests = 100; opt.WindowSeconds = 60; })
    .AddOksPerformance(opt => { opt.ThresholdMilliseconds = 500; });

builder.Services.AddOksExceptionHandling();
builder.Services.AddOksRequestLogging();

var app = builder.Build();

app.UseOksExceptionHandling();
app.UseOksRequestLogging();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

## 4) Custom log yazımı
```csharp
using Oks.Logging.Abstractions.Enums;
using Oks.Logging.Abstractions.Interfaces;
using Oks.Logging.Abstractions.Models;

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

## 5) Migration komutları
```powershell
Add-Migration InitOksLogs
Update-Database
```

Bu adımların ardından tüm log kategorileri otomatik olarak çalışır; ihtiyacına göre servis kayıtlarını açıp kapatabilirsin.
