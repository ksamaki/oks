# Logging Modülü Açıklaması

OKS Logging modülü, uygulamadaki istek, hata ve özel olayları standart bir formatta toplayarak merkezi log yönetimi sağlar. EF Core tabanlı eklentiler ile veritabanına yazabilir veya farklı sağlayıcılara yönlendirebilirsin.

> Navigasyon: [Kullanım Kılavuzu](Logging_Usage.md) | [README](README.md)

## Sağladıkları
- HTTP isteklerini, hataları ve performans verilerini otomatik toplar.
- Özel log girdileri için `IOksLogWriter` arabirimini sunar.
- İsteğe göre audit ve repository loglarını etkinleştirir.

## Ne Zaman Kullanmalı?
- Merkezi log toplama ve izleme ihtiyacın varsa.
- Uygulama hatalarını standart bir formatta depolamak istediğinde.
- Performans veya rate limit ihlallerini ölçmek istediğinde.
