# OKS Framework

Modern .NET 8+ uygulamalar için geliştirilmiş, modüler, genişletilebilir ve tamamen **opsiyonel bileşenlerden** oluşan bir uygulama çatısıdır.

OKS; Clean Architecture, SOLID, DI/IoC ve Middleware-Filter tabanlı modern tasarım yaklaşımlarına göre tasarlanmıştır.

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

## Kullanım Kılavuzu

- Proje açıklaması ve bileşen özeti: [OKS_DESCRIPTION.md](OKS_DESCRIPTION.md)
- Detaylı kullanım ve örnek kodlar: [OKS_USAGE.md](OKS_USAGE.md)

---

## Ana Özellikler

- **Tamamen modüler** - Ne eklersen o çalışır, eklemediğin hiçbir şey sistemi bozmaz.
- **SOLID & Clean Architecture uyumlu** katmanlar.
- **Opsiyonel log pipeline** - IOksLogWriter yoksa bile kod kırılmaz.
- **EF Core tabanlı repository & unit of work**.
- **Action başlamadan çalışan validation & filter mimarisi**.

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

Bu logların hepsi **opsiyoneldir**. Hangi log tipini kullanmak istiyorsan sadece onun DI extension'ını çağırırsın.

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

### DbContext içinde ModelBuilder Konfigürasyonu

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
```
