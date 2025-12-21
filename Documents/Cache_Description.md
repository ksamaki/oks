# Cache - Description

[Ana sayfa](../README.md)

OKS cache paketi; okuma trafiğini otomatik cache'leyip yazma tarafında tag/pattern tabanlı yırtmayı yöneten, attribute tabanlı ve genişletilebilir bir mekanizma sunar. Paket referans edilmediği sürece devre dışı kalır; eklendiğinde MVC filtreleri, repository dekoratörleri ve MediatR pipeline'ı ile minimum kodla devreye girer.

## Temel hedefler
- **Opsiyonel NuGet paketi**: Sadece `Oks.Caching*` paketleri eklendiğinde çalışır.
- **Attribute ile self-service**: `[Cacheable]` ve `[CacheEvict]` ile controller, minimal API ya da handler seviyesinde cache yönetimi.
- **Read/Write ayrımı**: Read repository dekoratörü cache'ler; Write tarafı entity tag'lerini yırtar.
- **Provider bağımsız**: `IMemoryCache` ve `IDistributedCache` (Redis) adaptörleri, ileride başka sağlayıcılarla genişleyebilir.
- **TTL ve stampede kontrolü**: Absolute/sliding expiration, soft-TTL + single-flight ile güvenli eşzamanlı erişim.
- **Gözlemlenebilirlik**: Hit/miss, set/remove olayları için log ve metrik hook noktaları.

## Başlıca bileşenler
- **Oks.Caching.Abstractions**: `ICacheService`, `CacheEntryOptions`, `CacheKey`, `[Cacheable]`, `[CacheEvict]`.
- **Oks.Caching**: `CacheService`, `ICacheSerializer`, `ICacheKeyBuilder`, Read/Write repository dekoratörleri, MVC/Minimal API filtreleri.
- **Oks.Caching.Redis**: `IDistributedCache` için Redis adaptörü.

## Mimari akış
1. `[Cacheable]` attribute veya Read repository dekoratörü, `ICacheService.GetOrAddAsync` ile cache kontrolü yapar.
2. Cache anahtarı; rota parametreleri, sorgu parametreleri, tenant/kullanıcı bilgisi gibi segmentlerden `ICacheKeyBuilder` ile deterministik üretilir.
3. Write repository ya da domain event handler'ı, etkilenen entity tag'lerini (`Entity`, `Entity:Id`, `Query:Entity`) `RemoveByTagAsync` ile temizler.
4. Soft-TTL ve single-flight kilidiyle aynı anda gelen yoğun okuma isteklerinde stampede önlenir.

## Kullanım senaryoları
- EfRead repository sonuçlarını cache'leyip aynı entity'nin yazma operasyonunda ilgili tag'leri otomatik yırtmak.
- API uçlarını `[Cacheable(KeyTemplate = "users/{id}", DurationSeconds = 300)]` ile cache'lemek.
- Komut/olay sonrası `[CacheEvict(Tags = new[] { "User", "User:{id}" })]` ile ilgili liste ve tekil cache'leri temizlemek.

---
## Usage

Kurulum ve kopyala-yapıştır kod örnekleri için: [Cache_Usage.md](Cache_Usage.md)
