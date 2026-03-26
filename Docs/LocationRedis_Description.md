# Location Redis - Description

[Ana sayfa](../README.md)

`Oks.Location.Redis`, artık yalnızca "location" domain'i ile sınırlı olmayacak şekilde framework genelinde kullanılabilen GeoSpatial kontratlar sunar.

## Başlıca bileşenler
- **Yeni genel kontratlar**
  - `IGeoSpatialWriteRepository<TEntity, TKey>`
  - `IGeoSpatialCache`
  - `IGeoSpatialMatcher`
  - `GeoCoordinate`, `GeoRadiusMatch`
- **Geriye uyumluluk katmanı**
  - `ILocationWriteRepository`, `ILocationGeoCache`, `IProximityMatcher`
  - `GeoPoint`, `ProximityMatch` (obsolete olarak korunur)

## Neler sağlar?
- Domain bağımsız GeoSpatial adlandırma.
- Mevcut implementasyonlar için kırılmayan geçiş.
- Redis/IDistributedCache tabanlı altyapı implementasyonlarını uygulamadan ayrıştırma.

> Not: Bu paket yalnızca sözleşme (interface + model) içerir; Redis client implementasyonu içermez.

---
## Usage

Kurulum ve örnek entegrasyon için: [LocationRedis_Usage.md](LocationRedis_Usage.md)
