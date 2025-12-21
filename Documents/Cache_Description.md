# Cache - Description

[Ana sayfa](../README.md)

OKS cache paketi; okuma trafiğini otomatik cache'leyip yazma tarafında tag/pattern tabanlı yırtmayı yöneten, attribute tabanlı ve genişletilebilir bir mekanizma sunar. Paket referans edilmediği sürece devre dışı kalır; eklendiğinde MVC filtreleri, repository dekoratörleri ve MediatR pipeline'ı ile minimum kodla devreye girer.

## Tasarım hedefleri
- **Opsiyonel NuGet paketi**: Sadece `Oks.Caching*` paketleri eklendiğinde çalışır.
- **Attribute ile self-service**: `[Cacheable]` ve `[CacheEvict]` ile controller, minimal API ya da handler seviyesinde cache yönetimi.
- **Read/Write ayrımı**: Read repository dekoratörü cache'ler; Write tarafı entity tag'lerini yırtar.
- **Sağlam cache anahtarları**: Tekil, liste ve sayfalı sorgular için deterministik key + isteğe bağlı tenant/kullanıcı segmentleri.
- **Provider bağımsız**: `IMemoryCache` ve `IDistributedCache` (Redis) adaptörleri; ileride başka sağlayıcılarla genişleyebilir.
- **TTL ve stampede kontrolü**: Absolute/sliding expiration, soft-TTL + single-flight ile güvenli eşzamanlı erişim.
- **Gözlemlenebilirlik**: Hit/miss, set/remove olayları için log ve metrik hook noktaları.

## Paketleme
```
Oks.Caching.Abstractions
  - ICacheService (GetAsync/SetAsync/RemoveAsync/GetOrAddAsync)
  - CacheEntryOptions (TTL, sliding, priority, tags)
  - CacheKey (segment tabanlı, hashing destekli)
  - Attributes: [Cacheable], [CacheEvict]

Oks.Caching (çekirdek)
  - CacheService (IMemoryCache & IDistributedCache implementasyonu)
  - ICacheSerializer (System.Text.Json tabanlı varsayılan)
  - ICacheKeyBuilder
  - Decorators: ReadRepositoryCacheDecorator, UnitOfWorkCacheEvictDecorator
  - Filters: CacheableActionFilter, CacheEvictActionFilter (MVC & Minimal API)

Oks.Caching.Redis (opsiyonel)
  - StackExchange.Redis tabanlı IDistributedCache adaptörü
```

## Mimari akış
1. `[Cacheable]` attribute veya Read repository dekoratörü, `ICacheService.GetOrAddAsync` ile cache kontrolü yapar.
2. Cache anahtarı; rota parametreleri, sorgu parametreleri, tenant/kullanıcı bilgisi gibi segmentlerden `ICacheKeyBuilder` ile deterministik üretilir.
3. Write repository, domain event handler veya `[CacheEvict]` attribute ile etkilenen entity tag'leri (`Entity`, `Entity:Id`, `Query:Entity`) temizlenir.
4. Soft-TTL ve single-flight kilidiyle aynı anda gelen yoğun okuma isteklerinde stampede önlenir.

## Önerilen API yüzeyi
```csharp
public interface ICacheService
{
    Task<T?> GetAsync<T>(CacheKey key, CancellationToken ct = default);
    Task SetAsync<T>(CacheKey key, T value, CacheEntryOptions? options = null, CancellationToken ct = default);
    Task RemoveAsync(CacheKey key, CancellationToken ct = default);
    Task RemoveByTagAsync(string tag, CancellationToken ct = default);
    Task<T> GetOrAddAsync<T>(CacheKey key, Func<Task<T>> factory, CacheEntryOptions? options = null, CancellationToken ct = default);
}

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public sealed class CacheableAttribute : Attribute
{
    public string KeyTemplate { get; init; } = string.Empty; // örn: "users/{id}/roles?page={page}&size={size}"
    public int DurationSeconds { get; init; } = 300;
    public string[] Tags { get; init; } = Array.Empty<string>();
}

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public sealed class CacheEvictAttribute : Attribute
{
    public string[] Tags { get; init; } = Array.Empty<string>();
    public bool EvictAllEntityCache { get; init; } // örn: Create/Update/Delete sonrası
}
```

## Repository entegrasyonu
- `ReadRepositoryCacheDecorator` veya `IReadRepository` extension'ı, her `GetAsync/FirstAsync/ListAsync/PagedAsync` çağrısı için cache key üretir.
- Key bileşenleri: `EntityName`, `Method`, `Ids`, `Filters` hash'i, `Page/Size`, `IncludeGraph` bilgisi.
- Varsayılan TTL: 5 dk (konfigüre edilebilir). `AsNoTracking` + cache ile birleşince okuma performansı artar.
- Batch read (`GetByIdsAsync`) için key; id listesi sıralanarak hash'lenir (deterministik anahtar).

## Yırtma stratejisi
- `WriteRepositoryCacheEvictDecorator` veya `UnitOfWorkCacheEvictBehavior` sınıfı; `Insert/Update/Delete/SoftDelete` işlemleri tamamlandığında tetiklenir.
- Yırtma stratejisi:
  - Tekil entity için: `tag = $"{EntityName}:{Id}"` ve `tag = EntityName`.
  - Liste cache'leri için: `tag = $"Query:{EntityName}"` gibi genel tag'ler.
  - Domain Event ile: `EntityChangedEvent<TEntity>` publish edilip `CacheInvalidationHandler` tarafından `RemoveByTagAsync` çağrılır.
- Transaction güvenliği: Yırtma işlemi `IUnitOfWork.CompleteAsync` sonrasında, transaction başarılıysa çalışmalı.

## Cache key üretim kuralları
- Segment bazlı: `oks:{area}:{resource}:{paramsHash}`.
- `paramsHash`: sorgu parametreleri deterministik sırayla JSON serialize edilip SHA256 hash alınır.
- Kullanıcı/tenant izolasyonu: isteğe bağlı `TenantId`, `UserId` segmentleri eklenebilir.
- Versioning: Schema değişikliğinde `CacheEntryOptions.Version` artırılarak eski entry'ler geçersiz kalır.

## Stampede ve konsistensi önlemleri
- **Soft TTL + background refresh**: TTL bitimine yakın kısa süreli uzatma (`refreshAheadSeconds`). İlk istek üretimi "single flight" lock ile korunur.
- **Error-aware caching**: Üretici hata verirse eski değer belirli süre daha kullanılabilir (stale-while-revalidate) ya da null döndürülür.
- **Priority & size limit**: Memory cache tarafında giriş önceliği ve boyut sınırlaması desteklenmeli.

## Konfigürasyon örneği (appsettings)
```json
"OksCaching": {
  "Provider": "Distributed", // Memory | Distributed
  "DefaultDurationSeconds": 300,
  "Sliding": false,
  "Tags": {
    "User": ["Query:User"],
    "Order": ["Query:Order"]
  }
}
```
DI kaydı:
```csharp
services.AddOksCaching(builder =>
{
    builder.UseDistributedCache(); // Redis adaptörü seçilir
    builder.WithDefaultOptions(o => { o.Duration = TimeSpan.FromMinutes(5); o.SoftTtl = TimeSpan.FromSeconds(30); });
    builder.AddReadRepositoryCaching();
    builder.AddWriteRepositoryEviction();
    builder.AddMvcFilters();
});
```

## Test stratejisi
- **Unit test**: Cache key üretimi, tag yırtma, GetOrAdd single-flight, TTL davranışı.
- **Integration test**: Repository dekoratörlerinin cache hit/miss ve yırtma sonuçları; EF Core + InMemory/Sqlite üzerinde.
- **Load test**: Stampede önleme doğrulaması (ör. 100 eşzamanlı okuma + tek seferde veri üretimi). Redis cluster ile latency ölçümü.

## Geliştirme yol haritası
1. `Oks.Caching.Abstractions` paketini tanımla; attribute'ları ve temel arayüzleri ekle.
2. `CacheService` ve `CacheKeyBuilder` implementasyonlarını yaz; memory + distributed destekle.
3. Read repository dekoratörünü entegre et; konfigürasyonla aç/kapa yap.
4. Write repository dekoratörü + domain event handler ile tag tabanlı yırtmayı uygula.
5. MVC/Minimal API filtresi ve MediatR pipeline behavior'larını ekle.
6. Telemetri hook'larını (`IOksLogWriter` veya `IMeter`) bağla; metrik/trace üret.
7. Belgelendirme ve örnek projede kullanım senaryosu ekle; NuGet paketlerini yayınla.

Bu tasarım, OKS'nin mevcut Read/Write repository ayrımına uyumlu şekilde, modern .NET 8 uygulamalarında minimum konfigürasyonla cache katmanını devreye almayı hedefler.

---
## Usage

Kurulum ve kopyala-yapıştır kod örnekleri için: [Cache_Usage.md](Cache_Usage.md)
