# WriteRepository Modülü Açıklaması

OKS WriteRepository modülü, veri ekleme, güncelleme ve silme işlemlerini transaction desteğiyle standartlaştıran yazma odaklı repository altyapısını sağlar.

> Navigasyon: [Kullanım Kılavuzu](WriteRepository_Usage.md) | [README](README.md)

## Sağladıkları
- `IWriteRepository<TEntity, TKey>` ile ekleme/güncelleme/silme operasyonları.
- Unit of Work ile entegre olarak otomatik transaction yönetimi.
- Soft delete ve audit alanlarını destekleyen taban sınıflarla uyumlu.

## Ne Zaman Kullanmalı?
- Yazma işlemlerini tek bir katmanda toplayarak kod tekrarını azaltmak istediğinde.
- Transaction ve audit kurallarını merkezi olarak uygulamak gerektiğinde.
