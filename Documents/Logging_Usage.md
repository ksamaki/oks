# Logging Modülü Kullanım Kılavuzu

Bu rehber, OKS Logging modülünü projene ekleyip istek, hata ve özel logları toplamak için gerekli temel adımları özetler.

> Navigasyon: [Açıklama](Logging_Description.md) | [README](README.md)

## Kurulum
1. `Oks.Logging.Abstractions`, `Oks.Logging` ve `Oks.Logging.EfCore` paketlerini projene ekle.
2. `DbContext` içinde `modelBuilder.AddOksLogging()` çağrısı yaparak log tablolarını modele ekle.
3. DI kayıtları için `builder.Services.AddOksLogging<AppDbContext>()` çağır.

## Kullanım
- Middleware olarak `UseOksExceptionHandling()` ve `UseOksRequestLogging()` ekle.
- Repository loglarını açmak için `AddOksRepositoryLogging` seçeneklerini ayarla.
- Özel log yazmak için `IOksLogWriter` bağımlılığını enjekte et.

## İpuçları
- Üretim ortamlarında hassas verileri maskelemek için `ExtraDataJson` alanını kontrollü kullan.
- Performans ve rate limit loglarını yalnızca ihtiyaç duyulduğunda etkinleştir.
