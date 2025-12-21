# Read-only Repository - Description

OKS read-only repository katmanı, veri okumayı yazma işlerinden ayırarak CQRS dostu ve test edilebilir bir altyapı sağlar. `IReadRepository<TEntity, TKey>` arabirimi; tekil kayıt, liste, sayfalama ve sorgu desteğini soyutlar. EF Core implementasyonu query filtreleri (soft delete) ve `AsNoTracking` varsayılanlarıyla gelir.

## Başlıca bileşenler
- **Oks.Persistence.Abstractions**: `IReadRepository` arabirimi ve ortak modeller.
- **Oks.Persistence.EfCore**: EF Core implementasyonu, DbContext taban sınıfı (`OksDbContextBase`) ve soft delete + audit alt yapısı.
- **Oks.Web.Abstractions**: HTTP pipeline için ortak filtre/attribute sözleşmeleri.

## Ne zaman kullanılır?
- Sadece okuma yapan API uçları için hafif ve güvenli repository erişimi istediğinde.
- Soft delete uygulanmış tablolarda otomatik `IsDeleted = false` filtresiyle çalışmak için.
- Yazma yetkisi olmayan servisler veya read-model uygulamaları için.

---
## Usage

Kurulum ve kopyala-yapıştır kod örnekleri için: [ReadRepository_Usage.md](ReadRepository_Usage.md)
