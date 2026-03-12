# Real-Time SignalR - Description

[Ana sayfa](../README.md)

`Oks.RealTime.SignalR`, WaitMe gibi anlık mesajlaşma senaryolarında SignalR bağlantı yaşam döngüsünü merkezi hale getirmek için tanımlanmış **kontrat paketidir**. Paket, JWT tabanlı kullanıcı doğrulamasını connection giriş noktasında standardize eder ve bağlantı/grup yetkilendirme kararlarını tek bir policy katmanına taşır.

## Başlıca bileşenler
- **`IOksRealtimeJwtPrincipalResolver`**: Access token'dan kullanıcı kimliğini çözümleyen sözleşme.
- **`IOksRealtimeAuthorizationPolicy`**: Kullanıcının hub'a bağlanma ve gruba katılma yetkisini doğrulayan sözleşme.
- **`IOksRealtimeConnectionStore`**: Aktif bağlantıları kullanıcı bazında takip eden sözleşme (tek cihaz/çok cihaz senaryoları için).
- **`IOksRealtimeHubSessionService`**: `OnConnected/OnDisconnected` akışını standartlaştıran üst servis sözleşmesi.

## Neler sağlar?
- Hub bazında tekrar eden JWT doğrulama kodlarını merkezileştirir.
- Kullanıcı başına aktif connection yönetimini (multi-device) standartlaştırır.
- Grup üyeliği kararlarını tek bir policy kontratına toplar.
- Uygulama katmanında ve altyapı katmanında bağımsız implementasyon geliştirmeye izin verir.

> Not: Bu paket yalnızca kontrat (interface + model) içerir; doğrudan SignalR runtime davranışı eklemez.

---
## Usage

Kurulum ve örnek entegrasyon için: [RealTimeSignalR_Usage.md](RealTimeSignalR_Usage.md)
