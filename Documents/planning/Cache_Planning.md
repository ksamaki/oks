# OKS Generic Cache Mekanizması - Tasarım

Bu doküman, OKS frameworküne **nuget paketi olarak eklenebilir** bir cache yeteneğinin nasıl tasarlanacağını anlatır. Amaç; okuma işlemlerini otomatik cache'leyen, yazma tarafında cache yırtılmasını (invalidation) kendi yöneten, attribute tabanlı ve genişletilebilir bir mekanizma kurmaktır.

## Tasarım hedefleri
- **Opsiyonel paket**: Sadece ilgili NuGet paketi referans edildiğinde devreye girmeli.
- **Attribute ile self-service**: Okuma tarafında cache'e alma, yazma tarafında yırtma senaryoları attribute/filtreden otomatik çalışmalı.
- **Read vs Write ayrımı**: Read-only repository çıktıları cache'lensin; Write repository ve Domain event'leri yırtma tetiklesin.
- **Sağlam cache anahtarları**: Tekil sorgular, listeleme, sayfalama ve custom query için deterministik cache key formatı.
- **Provider bağımsız**: `IMemoryCache` (in-memory) ve `IDistributedCache` (Redis) adaptörleriyle çalışabilir. Gelecekte başka provider'lar (Memcached, SQL) eklenebilir.
- **Thread-safe & TTL destekli**: Concurrency'de cache stampede önleyici mekanizmalar (lock + kısa süreli "soft-TTL"), mutabakatlı süre sonu (absolute + sliding expiration).
- **Observability**: Hit/miss oranı, set/remove olayları için log ve metriğe hook noktaları.

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
1. **Cacheable attribute** (`[Cacheable(Key = "users/{id}", DurationSeconds = 300)]`) action/service methodu veya Read repository metodunu sarmalar.
2. Attribute, `CacheableActionFilter` üzerinden `ICacheService` ile `GetOrAddAsync` çağırır. Cache key; rota parametreleri + query + kullanıcı kimliği gibi segmentlerden `ICacheKeyBuilder` ile üretilir.
3. **ReadRepositoryCacheDecorator**, `IReadRepository` implementasyonunu sarmalar. `GetAsync`, `ListAsync`, `PagedAsync` gibi çağrılar cache'e alınır; `Cacheable` attribute yoksa konvansiyonel key (entity adı + yöntem + parametre hash) üretilir.
4. **Write tarafı yırtma**: `IWriteRepository` dekoratörü, `Insert/Update/Delete/SoftDelete` sonrası etkilenen entity type + id bilgisini `ICacheService.RemoveByTagAsync` (veya key pattern) ile yırtar. Domain Event handler veya `IUnitOfWork.CompleteAsync` pipeline'ında da tetiklenebilir.
5. **Tag/Pattern tabanlı yırtma**: Cache entry'leri entity adı ve isteğe bağlı etiketlerle tutulur (`tags: ["User", "User:42", "Query:User:Active"]`). Yazma işlemleri ilgili tag'leri temizler, böylece hem tekil hem liste cache'leri yırtılır.

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

## Read-only repository ile entegrasyon
- `ReadRepositoryCacheDecorator` ya da `IReadRepository` extension'ı, her `GetAsync/FirstAsync/ListAsync/PagedAsync` için cache key üretir.
- Key bileşenleri: `EntityName`, `Method`, `Ids`, `Filters` hash'i, `Page/Size`, `IncludeGraph` bilgisi.
- Varsayılan TTL: 5 dk (konfigüre edilebilir). `AsNoTracking` + cache ile birleşince okuma performansı artar.
- Batch read (`GetByIdsAsync`) için key; id listesi sıralanarak hash'lenir (deterministik anahtar).

## Write repository ile yırtma
- `WriteRepositoryCacheEvictDecorator` veya `UnitOfWorkCacheEvictBehavior` sınıfı; `Insert/Update/Delete/SoftDelete` işlemleri tamamlandığında tetiklenir.
- Yırtma stratejisi:
  - Tekil entity için: `tag = $"{EntityName}:{Id}"` ve `tag = EntityName`.
  - Liste cache'leri için: `tag = $"Query:{EntityName}"` gibi genel tag'ler.
  - Domain Event ile: `EntityChangedEvent<TEntity>` publish edilip `CacheInvalidationHandler` tarafından `RemoveByTagAsync` çağrılır.
- Transaction güvenliği: Yırtma işlemi `IUnitOfWork.CompleteAsync` sonrasında, transaction başarılıysa çalışmalı.

## Attribute çalıştırma modeli
- **MVC/Minimal API Filter**: `CacheableActionFilter` action parametrelerini okuyup key template'i doldurur, cache kontrolü yapar. `CacheEvictActionFilter` ise action tamamlandığında ilgili tag'leri temizler.
- **MediatR Pipeline** (opsiyonel): Query handler'lar için `[Cacheable]`, Command handler'lar için `[CacheEvict]` attribute'ları pipeline behavior üzerinden uygulanabilir.

## Cache key üretim kuralları
- Segment bazlı: `oks:{area}:{resource}:{paramsHash}`.
- `paramsHash`: sorgu parametreleri deterministik sırayla JSON serialize edilip SHA256 hash alınır.
- Kullanıcı/tenant izolasyonu: isteğe bağlı `TenantId`, `UserId` segmentleri eklenebilir.
- Versioning: Schema değişikliğinde `CacheEntryOptions.Version` artırılarak eski entry'ler geçersiz kalır.

## Stampede ve konsistensi önlemleri
- **Soft TTL + Background refresh**: TTL bitimine yakın kısa süreli uzatma (`refreshAheadSeconds`). İlk istek üretimi "single flight" lock ile korunur.
- **Error-aware caching**: Üretici hata verirse eski değer belirli süre daha kullanılabilir (stale-while-revalidate yaklaşımı) ya da null döndürülür.
- **Priority & Size limit**: Memory cache tarafında giriş önceliği ve boyut sınırlaması desteklenmeli.

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
