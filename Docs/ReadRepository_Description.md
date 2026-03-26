# Read-only Repository - Description

[Ana sayfa](../README.md)

OKS read-only repository katmanı, veri okumayı yazma işlerinden ayırarak CQRS dostu ve test edilebilir bir altyapı sağlar. `IReadRepository<TEntity, TKey>` artık hem `GetByIdAsync` hem de SQL'e çevrilebilir predicate alan `GetAsync` / `GetListAsync` overload'larını içerir.

## Başlıca bileşenler
- **Oks.Persistence.Abstractions**: `IReadRepository` arabirimi ve ortak modeller.
- **Oks.Persistence.EfCore**: EF Core implementasyonu, `AsNoTracking` varsayılanları ve soft-delete query filter.
- **Oks.Web**: HTTP tabanlı current user provider entegrasyonu (`IOksUserProvider`).

## Neler sağlar?
- Predicate'lerin DB tarafında çalışması (client-side evaluation riski azaltılır).
- `AsNoTracking` ile read performansı.
- Mevcut `GetByIdAsync` API ile geriye uyumluluk.
- `Query()` üzerinden sorting/paging için genişlemeye açık yapı.

---
## Usage

Kurulum ve kopyala-yapıştır kod örnekleri için: [ReadRepository_Usage.md](ReadRepository_Usage.md)
