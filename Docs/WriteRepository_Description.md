# Write Repository & Unit of Work - Description

[Ana sayfa](../README.md)

Yazma operasyonları için OKS, `IWriteRepository<TEntity, TKey>` ve `EfUnitOfWork` kombinasyonunu sunar. `AddOksUnitOfWork()` ile MVC action'larda filter, Minimal API endpoint'lerinde middleware üzerinden request sonunda otomatik `SaveChangesAsync` denenir; `[OksSkipTransaction]` ile davranış endpoint/action bazında kapatılabilir.

## Başlıca bileşenler
- **Oks.Persistence.Abstractions**: `IWriteRepository`, unit of work ve transaction kontratları.
- **Oks.Persistence.EfCore**: EF Core implementasyonları, audit ve soft delete desteği.
- **Oks.Web**: Unit of work filter/middleware ve transaction attribute'ları.
- **Oks.Domain**: `Entity`, `AuditedEntity` ve `IAuditedEntity` taban tipleri ile audit alanlarını standartlaştırır.

## Neler sağlar?
- Action sonunda otomatik commit, yazma olmadığında boş commit yapılmaz.
- MVC action'larda `OksUnitOfWorkFilter`, Minimal API'lerde `OksUnitOfWorkMiddleware` otomatik commit dener.
- `[OksSkipTransaction]` ile filtre tamamen devre dışı bırakılabilir (örneğin toplu import senaryoları).
- Audit alanları (`CreatedAt`, `CreatedBy`, `ModifiedAt`, `ModifiedBy`) ve soft delete otomatik doldurulur.

---
## Usage

Kurulum ve kopyala-yapıştır kod örnekleri için: [WriteRepository_Usage.md](WriteRepository_Usage.md)
