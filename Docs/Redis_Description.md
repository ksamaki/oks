# Redis - Description

[Redis - Usage](Redis_Usage.md) | [Ana sayfa](../README.md)

Bu doküman, `Oks.Caching` içinde Redis kullanımını ayrı bir rehber olarak açıklar.

## Amaç

Redis entegrasyonunun:
- framework seviyesinde tekrar kullanılabilir,
- environment bağımsız konfigüre edilebilir,
- memory/distributed provider arasında kolay geçiş yapılabilir
olmasını sağlar.

## İlgili bileşenler

- `Oks.Caching.OksCachingOptions`
- `Oks.Caching.RedisCacheOptions`
- `Oks.Caching.Extensions.OksCachingOptionsExtensions`
  - `UseDistributedCache(...)`
  - `UseRedis(configuration, instanceName)`
- `Oks.Caching.Extensions.OksCachingServiceCollectionExtensions`
  - `AddOksCaching(...)`

## Çalışma modeli

1. `AddOksCaching(...)` çağrılır.
2. `UseRedis(...)` veya `UseDistributedCache(...)` ile distributed provider seçilir.
3. Eğer Redis konfigürasyonu verilmişse `AddStackExchangeRedisCache(...)` otomatik bağlanır.
4. Redis ayarı yok ama distributed provider seçiliyse fallback olarak `AddDistributedMemoryCache()` kullanılır.

## Hangi durumda hangi yöntem?

- `UseRedis(...)`: En kısa ve standart yol (connection string + instance name).
- `UseDistributedCache(...)`: Host uygulamanın provider kaydını tamamen kendisinin yapmak istediği gelişmiş yol.

## Güvenlik ve operasyon notları

- Redis connection bilgisi `appsettings` yerine secret manager/vault üzerinden yönetilmeli.
- Üretimde prefix (`InstanceName`) kullanımı ile key çakışmaları önlenmeli.
- TTL stratejileri (`DefaultEntryOptions`) ortama göre ayarlanmalı.
- Failover/Sentinel/Cluster gibi topolojilerde connection config host tarafından verilmeli.

---
## Usage

Kurulum ve örnekler için: [Redis_Usage.md](Redis_Usage.md)
