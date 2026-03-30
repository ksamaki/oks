# Cache - Description

[Cache - Usage](Cache_Usage.md) | [Ana sayfa](../README.md)

Bu doküman, OksFramework cache altyapısının **Entity-level cache** ve **Query-level cache** olarak ayrıştırılmış yeni mimarisini açıklar.

## 1) Tasarım hedefi
- Entity-level cache (`[OksEntityCache]`) yalnızca entity class’larda çalışır.
- Query-level cache (`[OksCache]`) yalnızca method seviyesinde çalışır (service/application/repository).
- Invalidate işlemleri key bazlı değil, **dependency/tag bazlı** yapılır (`[OksCacheInvalidate]`).
- Distributed senaryoda Redis ile tag->key eşlemesi yönetilebilir.
- Cache-aside pattern, TTL, empty-result cache, stampede protection desteklenir.

## 2) Attribute modeli

### Entity-level
- `OksEntityCacheAttribute`
- `[AttributeUsage(AttributeTargets.Class)]`
- Method/service üstünde kullanılamaz (compile-time kısıtı).
- Write repository (insert/update/delete) sonrası entity ilişkili tag’ler otomatik invalidate edilir.

### Query-level
- `OksCacheAttribute`
- `[AttributeUsage(AttributeTargets.Method)]`
- Entity class üstünde kullanılamaz (compile-time kısıtı).
- Parametre tabanlı key/tag template çözer (`friends:list:user:{userId}`).

### Invalidate
- `OksCacheInvalidateAttribute`
- `[AttributeUsage(AttributeTargets.Method, AllowMultiple=true)]`
- Bir method birden fazla tag invalidate edebilir.
- Wildcard invalidate desteklenir (`user-friends:*`).

## 3) Temel interface’ler
- `ICacheKeyGenerator`
  - Method parametrelerinden template çözümü ve key/tag üretimi.
- `ICacheDependencyManager`
  - key<->tag mapping, resolve, tag temizleme.

## 4) Redis uyumu
- `RedisCacheDependencyManager`
- Tag mapping Redis Set üstünden tutulur (`oks:cache:tag:{tag}` -> keys).
- Tek tag veya wildcard pattern ile ilgili tüm key’ler bulunup invalidate edilir.

## 5) Interceptor/Pipeline
- MVC: `OksCustomCacheFilter`
- Minimal API: `OksMinimalApiCustomCacheFilter`
- Ortak executor: `OksQueryCacheExecutor`
  - Önce `[OksCacheInvalidate]` tag’lerini temizler.
  - Sonra `[OksCache]` için cache-aside çalıştırır.

## 6) Varsayılanlar
- TTL verilmezse `OksCachingOptions.DefaultEntryOptions.AbsoluteExpirationRelativeToNow` kullanılır.
- Stampede protection default `true`.
- Empty-result cache default `false`.
