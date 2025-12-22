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

> NOTE: Bazı ileri seviye özellikler (ör. Single-flight timeout, background refresh / refreshAheadSeconds) tasarımda belirtilmiştir ancak bu PR'ın kod içeriğinde tam olarak implement edilmemiştir. Bu özellikler ilerleyen PR'larda eklenecektir.

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
```

(...content continues unchanged...)

(Additionally added a short note near key/hash rules):

## Cache key üretim kuralları
- Segment bazlı: `oks:{area}:{resource}:{paramsHash}`.
- `paramsHash`: sorgu parametreleri deterministik sırayla JSON serialize edilip SHA256 hash alınır.

> NOTE: CacheKey.Hash serileştirmesinde kullanılan Json seçeneklerinin (PropertyNamingPolicy vb.) DefaultCacheSerializer ile tutarlı olması önemlidir; aksi takdirde farklı serileştirme ayarları farklı hashler üretebilir. İlerleyen PR'larda bu seçeneklerin paylaşılan bir Json konfigürasyonuna taşınması önerilmektedir.

(...rest of document unchanged...)
