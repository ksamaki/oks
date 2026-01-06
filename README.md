# OKS Framework

CI: [.github/workflows/ci.yml](.github/workflows/ci.yml)

Modern .NET 8+ uygulamalar için geliştirilmiş, modüler, genişletilebilir ve tamamen **opsiyonel bileşenlerden** oluşan bir uygulama çatısıdır. OKS; Clean Architecture, SOLID, DI/IoC ve mimarlar tarafından tercih edilen yaklaşımları destekler.

Amaç; yeni projelerde tekrar tekrar yazılan logging, rate limiting, validation, repository & unit of work, exception handling gibi altyapıları **tek satır konfigürasyon ile kullanılabilir** hale getirmektir.

---
## Özellikler ve dokümantasyon

Her yetenek için iki doküman bulunur: bir **Description** dosyası kavramsal detayları açıklar, **Usage** dosyası ise kopyala-yapıştır ile projene ekleyebileceğin kod parçalarını içerir.

- **Read-only Repository**: [docs/ReadRepository_Description.md](docs/ReadRepository_Description.md)
- **Write Repository & Unit of Work**: [docs/WriteRepository_Description.md](docs/WriteRepository_Description.md)
- **Logging (Request, Exception, Performance, RateLimit, Repository, Audit, Custom)**: [docs/Logging_Description.md](docs/Logging_Description.md)
- **Validation (FluentValidation)**: [docs/Validation_Description.md](docs/Validation_Description.md)
- **Cache (Read cache + Write eviction)**: [docs/Cache_Description.md](docs/Cache_Description.md)

WaitMe: [docs/waitme/README.md](docs/waitme/README.md) — WaitMe minimal API ve UI iskeleti.

Sprint planları ve ADR'ler: [docs/plan/](docs/plan/) | [docs/adr/](docs/adr/)

---
## Temel yapı taşları

- **Oks.Domain**: Base entity tipleri (`Entity`, `AuditedEntity`, `IAuditedEntity`).
- **Oks.Shared**: Ortak sonuç modelleri (`Result`, `DataResult`, `PagedDataResult`).
- **Oks.Persistence.Abstractions / Oks.Persistence.EfCore**: Repository, unit of work, audit ve soft delete altyapısı.
- **Tamamen modüler**: Yalnızca eklediğin extension ve servisler devreye girer.
- **SOLID & Clean Architecture uyumu**: Katmanlı tasarım ve arayüzler ile esnek kullanım.
- **Opsiyonel log pipeline**: IOksLogWriter yoksa bile kod kırılmaz; eklendiğinde tüm log çeşitleri otomatik çalışır.
- **EF Core tabanlı repository & unit of work**: Okuma-yazma ayrımı, audit ve soft delete desteği.
- **Action öncesi validation & filter mimarisi**: FluentValidation ile entegre, attribute ile aç/kapat esnekliği.

---
## Başlangıç noktası

- Framework'ü hızla kurmak için ilgili **Usage** dokümanındaki proje referanslarını, `DbContext` konfigürasyonunu ve DI eklentilerini doğrudan kopyala.
- Logging tabloları, unit of work filtresi, validation ve repository örnekleri için yukarıdaki özellik bağlantılarını takip et.

OKS ile ihtiyacın olan bileşeni seçip ekleyebilir, diğerlerini devre dışı bırakabilirsin.
