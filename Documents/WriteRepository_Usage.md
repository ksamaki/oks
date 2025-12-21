# WriteRepository Modülü Kullanım Kılavuzu

Bu rehber, `IWriteRepository` ve Unit of Work ile yazma işlemlerini nasıl yöneteceğini anlatır.

> Navigasyon: [Açıklama](WriteRepository_Description.md) | [README](README.md)

## Kurulum
1. `Oks.Persistence.Abstractions` veya `Oks.Persistence.EfCore` paketlerini ekle.
2. `builder.Services.AddOksEfCore<AppDbContext>()` ile repository ve unit of work kayıtlarını yap.
3. Controller'larında `AddOksUnitOfWork()` filtresiyle otomatik `SaveChanges` davranışını etkinleştir.

## Kullanım
- `IWriteRepository<TEntity, TKey>`'i enjekte ederek `AddAsync`, `UpdateAsync`, `DeleteAsync` metotlarını kullan.
- Transaction gerektiren senaryolarda `[OksTransactional]` veya `[OksSkipTransaction]` niteliklerini uygula.
- Soft delete kullanan varlıklar için `AuditedEntity` taban sınıfını tercih et.

## İpuçları
- Değişiklikleri kaydetmek için Unit of Work filtresinin devreye girdiğinden emin ol.
- Büyük toplu işlemlerde `SaveChangesAsync` çağrılarını kontrollü planla.
