# OKS Framework

Modern .NET 8+ uygulamalarý için geliþtirilmiþ, modüler, geniþletilebilir ve tamamen **opsiyonel bileþenlerden** oluþan bir uygulama çatýsýdýr.

OKS; Clean Architecture, SOLID, DI/IoC ve Middleware–Filter tabanlý modern tasarým yaklaþýmlarýna göre tasarlanmýþtýr.

Amaç; yeni projelerde tekrar tekrar yazýlan:

- Logging
- Rate limiting
- Performance monitoring
- Request tracing
- Exception handling
- Repository & Unit of Work
- Entity auditing

gibi altyapýlarý, **tek satýr konfigurasyon ile kullanýlabilir** hale getirmektir.

---

## Ana Özellikler

- **Tamamen modüler** – Ne eklersen o çalýþýr, eklemediðin hiçbir þey sistemi bozmaz.
- **SOLID & Clean Architecture uyumlu** katmanlar.
- **Opsiyonel log pipeline** – IOksLogWriter yoksa bile kod kýrýlmaz.
- **EF Core tabanlý repository & unit of work**.
- **Action baþlamadan çalýþan validation & filter mimarisi**.

---

## Log Tipleri

OKS þu log kategorilerini destekler:

| Log Tipi      | Açýklama |
|---------------|----------|
| **Request**   | Tüm HTTP istekleri (path, method, status, süre, client ip, vs.) |
| **Exception** | Global yakalanmamýþ hatalar |
| **Performance** | Controller action süreleri ve threshold aþýmý |
| **RateLimit** | Rate limit ihlalleri (429) |
| **Repository** | EfRead/EfWrite operasyon süreleri (Read/Write) |
| **Audit**     | Entity Insert / Update / Delete deðiþiklikleri |
| **Custom**    | Kod içinden IOksLogWriter ile atýlan özel loglar |

Bu loglarýn hepsi **opsiyonel**dir. Hangi log tipini kullanmak istiyorsan sadece onun DI extension'ýný çaðýrýrsýn.

---

## Log Tablolarý ve Migration

`Oks.Logging.EfCore` içerisinde aþaðýdaki log tablolarý tanýmlýdýr:

- `OksLogRequest`
- `OksLogException`
- `OksLogPerformance`
- `OksLogRateLimit`
- `OksLogRepository`
- `OksLogAudit`
- `OksLogCustom`

`ModelBuilderExtensions.AddOksLogging(modelBuilder)` çaðrýldýðýnda bu tablolar EF modeline dahil olur.

### DbContext Ýçinde ModelBuilder Konfigürasyonu

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

        // OKS log tablolarý
        modelBuilder.AddOksLogging();
    }
}
