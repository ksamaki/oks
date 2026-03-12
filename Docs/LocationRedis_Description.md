# Location Redis - Description

[Ana sayfa](../README.md)

`Oks.Location.Redis`, WaitMe proximity/yakınlık hesapları için Redis GeoSpatial kullanımını standardize eden **kontrat paketidir**. Amaç; konum güncellemelerini `IWriteRepository` akışıyla uyumlu sürdürmek ve Redis tarafında `IDistributedCache` standardı etrafında eşleştirme kontratları sağlamaktır.

## Başlıca bileşenler
- **`ILocationWriteRepository<TEntity, TKey>`**: `IWriteRepository` tabanlı yazma kontratına konum güncelleme davranışı ekler.
- **`ILocationGeoCache`**: Redis GEOADD/GEORADIUS benzeri operasyonların provider-bağımsız kontratı.
- **`IProximityMatcher`**: `IDistributedCache` erişimiyle proximity sorgulamasını aynı servis yüzeyinde toplar.

## Neler sağlar?
- Domain yazma akışını bozmadan konum güncelleme standardı (`UpdateLocationAsync`).
- Yakın kullanıcı aramalarında ortak metod imzaları (`FindNearbyAsync`, `SearchRadiusAsync`).
- Altyapı implementasyonlarını (StackExchange.Redis vb.) uygulamadan ayrıştırır.

> Not: Bu paket yalnızca sözleşme (interface + model) içerir; Redis client implementasyonu içermez.

---
## Usage

Kurulum ve örnek entegrasyon için: [LocationRedis_Usage.md](LocationRedis_Usage.md)
