# Write Repository & Unit of Work - Description

[Ana sayfa](../README.md)

Yazma operasyonları için OKS, `IWriteRepository<TEntity, TKey>` ve `EfUnitOfWork` kombinasyonunu sunar. `AddOksUnitOfWork()` ile MVC action'larda filter üzerinden otomatik `SaveChangesAsync` denenir; Minimal API tarafında aynı davranış `IEndpointFilter` tabanlı `AddOksUnitOfWork()` (RouteGroupBuilder extension) ile sağlanır. `[OksSkipTransaction]` ile davranış endpoint/action bazında kapatılabilir.

## Başlıca bileşenler
> Mimari not (2026-03-30): `Oks.Web` write pipeline tarafında yalnızca abstraction katmanlarına bağımlıdır; `IUnitOfWork`/repository concrete kayıtları host tarafında (`Oks.Persistence.EfCore` veya alternatif implementasyon) compose edilir.

- **Oks.Persistence.Abstractions**: `IWriteRepository`, unit of work ve transaction kontratları.
- **Oks.Persistence.EfCore**: EF Core implementasyonları, audit ve soft delete desteği.
- **Oks.Web**: MVC filter + Minimal API endpoint filter ile otomatik commit davranışı.
- **Oks.Domain**: `Entity`, `AuditedEntity` ve `IAuditedEntity` taban tipleri ile audit alanlarını standartlaştırır.

## Neler sağlar?
- Action/endpoint sonunda otomatik commit, yazma olmadığında boş commit yapılmaz.
- MVC action'larda `OksUnitOfWorkFilter`, Minimal API'lerde `OksMinimalApiUnitOfWorkFilter` otomatik commit dener.
- `[OksSkipTransaction]` ile davranış tamamen devre dışı bırakılabilir.
- Audit alanları (`CreatedAt`, `CreatedBy`, `ModifiedAt`, `ModifiedBy`) ve soft delete otomatik doldurulur.

---
## Usage

Kurulum ve kopyala-yapıştır kod örnekleri için: [WriteRepository_Usage.md](WriteRepository_Usage.md)
