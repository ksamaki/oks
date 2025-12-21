# ReadRepository Modülü Kullanım Kılavuzu

Bu rehber, `IReadRepository` arabirimiyle veri okuma işlemlerini nasıl standartlaştırabileceğini ve performanslı sorgular oluşturabileceğini gösterir.

> Navigasyon: [Açıklama](ReadRepository_Description.md) | [README](README.md)

## Kurulum
1. `Oks.Persistence.Abstractions` veya `Oks.Persistence.EfCore` paketlerini projene ekle.
2. DI kayıtları için `builder.Services.AddOksEfCore<AppDbContext>()` çağrısı yap.
3. DbContext'ini `OksDbContextBase`'ten türeterek altyapıyı etkinleştir.

## Kullanım
- `IReadRepository<TEntity, TKey>`'i controller veya servislere enjekte et.
- `GetAsync`, `GetListAsync`, `AnyAsync` gibi metotlarla veri oku.
- Gelişmiş filtreler için `Query()` metodu üzerinden LINQ sorgularını zincirle.

## İpuçları
- Büyük koleksiyonlar için sayfalama metotlarını (`GetPagedListAsync`) kullan.
- Gerektiğinde `Include`/`ThenInclude` ile ilişkileri yükle, ancak performansa dikkat et.
