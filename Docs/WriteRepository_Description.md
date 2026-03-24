# Write Repository & Unit of Work - Description

[Ana sayfa](../README.md)

Yazma operasyonları için OKS, `IWriteRepository<TEntity, TKey>` ve `EfUnitOfWork` kombinasyonunu sunar. HTTP pipeline'a eklenen `OksUnitOfWorkFilter`, action sonunda otomatik `SaveChangesAsync` çağırır; `[OksSkipTransaction]` attribute'u ile bu davranış action/controller bazında kapatılabilir.

## Başlıca bileşenler
- **Oks.Persistence.Abstractions**: `IWriteRepository`, unit of work ve transaction kontratları.
- **Oks.Persistence.EfCore**: EF Core implementasyonları, audit ve soft delete desteği.
- **Oks.Web**: Unit of work filter'ı ve transaction attribute'ları.
- **Oks.Domain**: `Entity`, `AuditedEntity` ve `IAuditedEntity` taban tipleri ile audit alanlarını standartlaştırır.

## Neler sağlar?
- Action sonunda otomatik commit, yazma olmadığında boş commit yapılmaz.
- `OksUnitOfWorkFilter`, başarılı action'larda otomatik commit dener ve `IUnitOfWork` tarafında değişiklik yoksa no-op olur.
- `[OksSkipTransaction]` ile filtre tamamen devre dışı bırakılabilir (örneğin toplu import senaryoları).
- Audit alanları (`CreatedAt`, `CreatedBy`, `ModifiedAt`, `ModifiedBy`) ve soft delete otomatik doldurulur.

---
## Usage

Kurulum ve kopyala-yapıştır kod örnekleri için: [WriteRepository_Usage.md](WriteRepository_Usage.md)
