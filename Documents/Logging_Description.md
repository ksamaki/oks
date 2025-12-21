# Logging - Description

OKS logging modülü; Request, Exception, Performance, RateLimit, Repository, Audit ve Custom kategorilerini destekleyen modüler bir pipeline sunar. IOksLogWriter uygulanmadığında bile uygulama kırılmaz; eklendiğinde otomatik olarak log yazmaya başlar.

## Log tipleri
| Log Tipi      | Açıklama |
|---------------|----------|
| **Request**   | Tüm HTTP istekleri (path, method, status, süre, client ip, vs.) |
| **Exception** | Global yakalanmamış hatalar |
| **Performance** | Controller action süreleri ve threshold aşımı |
| **RateLimit** | Rate limit ihlalleri (429) |
| **Repository** | EfRead/EfWrite operasyon süreleri (Read/Write) |
| **Audit**     | Entity Insert / Update / Delete değişiklikleri |
| **Custom**    | Kod içinden IOksLogWriter ile atılan özel loglar |

## Log tabloları ve migration
`Oks.Logging.EfCore` içerisinde aşağıdaki tablolar tanımlıdır:
- `OksLogRequest`
- `OksLogException`
- `OksLogPerformance`
- `OksLogRateLimit`
- `OksLogRepository`
- `OksLogAudit`
- `OksLogCustom`

`ModelBuilderExtensions.AddOksLogging(modelBuilder)` çağrıldığında bu tablolar EF modeline dahil olur ve standart EF Core migration'larına eklenir.

## Attribute desteği
- `[OksPerformance(ThresholdMilliseconds = 200)]` ile action bazında performans eşiği belirleyebilirsin.
- `[OksRateLimit(MaxRequests = 10, WindowSeconds = 60)]` ile belirli uçlara özel hız limiti koyabilirsin.
- `[OksSkipPerformance]` veya `[OksSkipRateLimit]` attribute'ları belirli sınıf/metotlarda ilgili logu devre dışı bırakır fakat işlem çalışmaya devam eder.

---
## Usage

Kurulum ve kopyala-yapıştır kod örnekleri için: [Logging_Usage.md](Logging_Usage.md)
