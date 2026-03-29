# OKS Framework

Modern .NET 8+ uygulamalar için geliştirilmiş, modüler, genişletilebilir ve tamamen **opsiyonel bileşenlerden** oluşan bir uygulama çatısıdır. OKS; Clean Architecture, SOLID, DI/IoC ve middleware-filter tabanlı modern tasarım yaklaşımlarına göre tasarlanmıştır.

Amaç; yeni projelerde tekrar tekrar yazılan logging, rate limiting, validation, repository & unit of work, exception handling gibi altyapıları **tek satır konfigurasyon ile kullanılabilir** hale getirmektir. Her özellik yalnızca eklendiğinde çalışır, eklenmediğinde sistemi bozmaz.

---
## Özellikler ve dokümantasyon

Her yetenek için iki doküman bulunur: bir **Description** dosyası kavramsal detayları açıklar, **Usage** dosyası ise kopyala-yapıştır ile projene ekleyebileceğin kod parçalarını içerir.

- **Read-only Repository**: [ReadRepository_Description.md](Docs/ReadRepository_Description.md)
- **Write Repository & Unit of Work**: [WriteRepository_Description.md](Docs/WriteRepository_Description.md)
- **Logging (Request, Exception, Performance, RateLimit, Repository, Audit, Custom)**: [Logging_Description.md](Docs/Logging_Description.md)
- **Validation (FluentValidation)**: [Validation_Description.md](Docs/Validation_Description.md)
- **Cache (Read cache + Write eviction)**: [Cache_Description.md](Docs/Cache_Description.md)
- **Redis (Oks.Caching distributed provider profili)**: [Redis_Description.md](Docs/Redis_Description.md)
- **Real-Time (SignalR + JWT connection contracts)**: [RealTimeSignalR_Description.md](Docs/RealTimeSignalR_Description.md)
- **Authentication (JWT default + optional OpenIddict + EF Core persistence)**: [Authentication_Description.md](Docs/Authentication_Description.md)

Her Description dokümanında ilgili Usage sayfasına bağlantıyı bulabilirsin.

- GitHub Packages ile NuGet yayınlama ve tüketim rehberi: [NuGet_GitHubPackages.md](Docs/NuGet_GitHubPackages.md)

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

- Frameworkü hızla kurmak için ilgili **Usage** dokümanındaki proje referanslarını, `DbContext` konfigürasyonunu ve DI eklentilerini doğrudan kopyala.
- Logging tabloları, unit of work filtresi, validation ve repository örnekleri için yukarıdaki özellik bağlantılarını takip et.

OKS ile ihtiyacın olan bileşeni seçip ekleyebilir, diğerlerini devre dışı bırakabilirsin.

---
## NuGet paketleme ve yayınlama (GitHub Packages)

OKS modülleri bağımsız NuGet paketleri olarak yayınlanıp (versioned) farklı bir repodan (ör. WaitMe) `PackageReference` ile tüketilebilir. İlk paket seti:

- `Oks.Persistence.Abstractions`
- `Oks.Persistence.EfCore`
- `Oks.Web.Validation`
- `Oks.Logging.Abstractions`
- `Oks.Logging`

### Publish akışı

1. Paket sürümünü `Directory.Build.props` içindeki `<Version>` alanında güncelleyin.
2. Değişiklikleri `main` dalına gönderin.
3. `.github/workflows/publish.yml` iş akışı restore, build, pack ve publish adımlarını otomatik çalıştırır.
4. İş akışı, `main` için benzersiz bir CI sürümü üretir (`<Version>-ci.<run_number>`).

Detaylı adımlar, CLI komutları ve troubleshooting için: [Docs/NuGet_GitHubPackages.md](Docs/NuGet_GitHubPackages.md).

### WaitMe tarafında tüketim

- WaitMe reposunda GitHub Packages kaynağını içeren bir `nuget.config` tanımlayın.
- İlgili OKS paketlerini `PackageReference` ile ekleyin.
- Detaylı örnek için: `docs/waitme-consumption.md`.
