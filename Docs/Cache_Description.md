# Cache - Description

[Cache - Usage](Cache_Usage.md) | [Ana sayfa](../README.md)

`Oks.Caching`, artık framework genelinde kullanılabilen tekil bir cache altyapısıdır. Redis sadece belirli bir domain'in parçası değil; tüm framework modülleri ve framework tüketen uygulamalar için yeniden kullanılabilir bir bileşen olarak konumlandırılmıştır.

## Temel hedef
- Varsayılan olarak **pasif** kalır, sadece attribute ile işaretlenen alanlarda çalışır.
- Entity seviyesinde `[Cacheable]` ile query tarafında otomatik `GetOrSet` çalışır.
- Command tarafında aynı entity için cache otomatik temizlenir (yırtılır).
- Ek iş akışları için method/action/class seviyesinde `[CustomCache]` ile davranış özelleştirilebilir.
- Minimal API ve MVC senaryolarında aynı yaklaşım desteklenir.

## Bileşenler
- `Oks.Caching.Abstractions`
  - `ICacheService`, `ICacheManager`
  - `CacheKey`, `CacheEntryOptions`
  - `[Cacheable]`, `[CacheEvict]`, `[CustomCache]`
- `Oks.Caching`
  - `CacheService` (Memory + Distributed)
  - `CacheManager` (elle cache yönetimi)
  - Repository dekoratörleri (`CachedReadRepository`, `CacheEvictingWriteRepository`)
  - Redis bağlantısı için `UseRedis(...)`
- `Oks.Web`
  - MVC: `OksCustomCacheFilter`
  - Minimal API: `OksMinimalApiCustomCacheFilter`

## Çalışma modeli
1. `AddOksCaching(...)` ile altyapı eklenir.
2. Entity üzerinde `[Cacheable]` varsa repository query'leri cache'e alınır.
3. Aynı entity üzerinde write olduğunda entity tag'leri otomatik temizlenir.
4. Endpoint/action seviyesinde `[CustomCache]` varsa response cache veya explicit evict uygulanır.
5. Elle kullanımda `ICacheManager` ile key/tag bazlı set/get/remove yapılabilir.

## Güvenlik ve performans
- Varsayılan davranış tüm endpointleri cache'lemek değildir.
- Soft TTL + single-flight ile stampede riski azaltılır.
- Memory veya distributed provider seçimi opsiyoneldir.
- Distributed provider tarafı extensible'dır: özel provider DI kaydı verilebilir, Redis ise `UseRedis(...)` ile hazır profil olarak kullanılabilir.
- Repository query cache kapsamı varsayılan olarak sadece liste sorgularıdır (`ListOnly`), istenirse `CacheAllRepositoryQueries()` ile genişletilebilir.
