# ReadRepository Modülü Açıklaması

OKS ReadRepository modülü, veri okuma operasyonlarını sadeleştiren ve sorguları merkezileştiren bir repository altyapısı sunar. IQueryable tabanlı esnek API'si ile LINQ sorgularını yeniden kullanılabilir hale getirir.

> Navigasyon: [Kullanım Kılavuzu](ReadRepository_Usage.md) | [README](README.md)

## Sağladıkları
- Tüm okuma operasyonları için ortak `IReadRepository<TEntity, TKey>` arabirimi.
- AsNoTracking varsayılan davranışı ile performans odaklı sorgular.
- Sayfalama ve filtreleme için yardımcı metotlar.

## Ne Zaman Kullanmalı?
- Sorgu mantığını servislerden ayırmak istediğinde.
- Test edilebilir ve tekrar kullanılabilir okuma katmanı kurmak istediğinde.
- IQueryable zincirlerini merkezi olarak yönetmek gerektiğinde.
