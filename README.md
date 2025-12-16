# OKS Framework

Modern .NET 8+ uygulamaları için geliştirilmiş, modüler, genişletilebilir ve tamamen **opsiyonel bileşenlerden** oluşan bir uygulama çatısıdır.

OKS; Clean Architecture, SOLID, DI/IoC ve Middleware–Filter tabanlı modern tasarım yaklaşımlarına göre tasarlanmıştır.

Amaç; yeni projelerde tekrar tekrar yazılan:

- Logging
- Rate limiting
- Performance monitoring
- Request tracing
- Exception handling
- Repository & Unit of Work
- Entity auditing

gibi altyapıları, **tek satır konfigurasyon ile kullanılabilir** hale getirmektir.

---

## Ana Özellikler

- **Tamamen modüler** – Ne eklersen o çalışır, eklemediğin hiçbir şey sistemi bozmaz.
- **SOLID & Clean Architecture uyumlu** katmanlar.
- **Opsiyonel log pipeline** – IOksLogWriter yoksa bile kod kırılmaz.
- **EF Core tabanlı repository & unit of work**.
- **Action başlamadan çalışan validation & filter mimarisi**.
- **İsteğe bağlı repository cache katmanı** (attribute + DI ile aktif edilir).

---

## Log Tipleri

OKS şu log kategorilerini destekler:

| Log Tipi      | Açıklama |
|---------------|----------|
| **Request**   | Tüm HTTP istekleri (path, method, status, süre, client ip, vs.) |
| **Exception** | Global yakalanmamış hatalar |
| **Performance** | Controller action süreleri ve threshold aşımı |
| **RateLimit** | Rate limit ihlalleri (429) |
| **Repository** | EfRead/EfWrite operasyon süreleri (Read/Write) |
| **Audit**     | Entity Insert / Update / Delete değişiklikleri |
| **Custom**    | Kod içinden IOksLogWriter ile atılan özel loglar |

Bu logların hepsi **opsiyonel**dir. Hangi log tipini kullanmak istiyorsan sadece onun DI extension'ını çağırırsın.

---

## Log Tabloları ve Migration

`Oks.Logging.EfCore` içerisinde aşağıdaki log tabloları tanımlıdır:

- `OksLogRequest`
- `OksLogException`
- `OksLogPerformance`
- `OksLogRateLimit`
- `OksLogRepository`
- `OksLogAudit`
- `OksLogCustom`

`ModelBuilderExtensions.AddOksLogging(modelBuilder)` çağrıldığında bu tablolar EF modeline dahil olur.

### DbContext İçinde ModelBuilder Konfigürasyonu

```csharp
using Microsoft.EntityFrameworkCore;
using Oks.Logging.EfCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // OKS log tabloları
        modelBuilder.AddOksLogging();
    }
}


## Repository Cache Kullanımı

Okuma odaklı `IReadRepository` çağrılarına bellek içi cache eklemek için:

1. Cache'e almak istediğiniz entity'yi işaretleyin:

```csharp
using Oks.Domain.Attributes;

[EnableRepositoryCache(absoluteExpirationSeconds: 300, SlidingExpirationSeconds = 60)]
public class Product : AuditedEntity<Guid>
{
    // ...
}
```

2. DI kayıtlarına cache katmanını ekleyin ve süreyi/global ayarları opsiyonel olarak yapılandırın:

```csharp
services
    .AddOksEfCore<AppDbContext>()
    .AddOksRepositoryCache(options =>
    {
        options.DefaultAbsoluteExpiration = TimeSpan.FromMinutes(5);
        options.DefaultSlidingExpiration = TimeSpan.FromMinutes(1);
    });
```

`AddOksRepositoryCache` çağrısı sonrası `IReadRepository` otomatik olarak cache'li sürümle değişir, `IWriteRepository` ise her yazma işleminde ilgili entity cache'ini temizler.
