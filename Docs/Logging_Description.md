# Logging - Description

[Ana sayfa](../README.md)

OKS logging modülü; **Request, Exception, Performance, RateLimit, Repository, Audit ve Custom** kategorilerini destekleyen modüler bir pipeline sunar. MVC ve Minimal API birlikte kullanılabilir.

## Log tipleri
| Log Tipi | MVC | Minimal API | Kaynak |
|---|---|---|---|
| Request | ✅ | ✅ | `OksRequestLoggingMiddleware` |
| Exception | ✅ | ✅ | `OksExceptionMiddleware` |
| Performance | ✅ | ✅ | MVC Filter + Minimal API `IEndpointFilter` |
| RateLimit | ✅ | ✅ | MVC Filter + Minimal API `IEndpointFilter` |
| Repository | ✅ | ✅ | `EfReadRepository` / `EfWriteRepository` |
| Audit | ✅ | ✅ | `EfUnitOfWork` |
| Custom | ✅ | ✅ | `IOksLogWriter` |

## Log tabloları
`Oks.Logging.EfCore`:
- `OksLogRequest`
- `OksLogException`
- `OksLogPerformance`
- `OksLogRateLimit`
- `OksLogRepository`
- `OksLogAudit`
- `OksLogCustom`

`modelBuilder.AddOksLogging()` çağrısı bu tabloları EF modeline ekler.

## Attribute / Metadata desteği
- `[OksPerformance(thresholdMs)]`
- `[OksSkipPerformance]`
- `[OksRateLimit(requestsPerMinute)]`
- `[OksSkipRateLimit]`

Bu metadata'lar MVC action/controller üzerinde attribute olarak, Minimal API tarafında `.WithMetadata(...)` ile kullanılabilir.

---
## Usage

Kurulum ve örnekler için: [Logging_Usage.md](Logging_Usage.md)


## Mimari not (ADR-2026-03-30)
- `Oks.Web` yalnızca `Oks.Logging.Abstractions` kontratlarına bağlıdır.
- `IOksLogWriter` concrete implementasyonu (örn. EF Core writer) host/integration katmanında ayrıca register edilmelidir.
- Bu nedenle `AddOksRequestLogging()`/`UseOksRequestLogging()` çağrıları korunur; ancak writer kaydı yapılmamışsa runtime DI hatası alınır.
