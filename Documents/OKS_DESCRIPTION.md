# OKS Framework Açıklaması

Bu doküman, OKS Framework'ün temel amaçlarını, mimari yaklaşımını ve hangi problemleri çözdüğünü kısa ve net bir şekilde özetler. Ayrıntılı kullanım ve örnekler için kullanım kılavuzuna geçebilirsiniz.

> Navigasyon: [Kullanım Kılavuzu](OKS_USAGE.md) | [README](README.md)

## Neden OKS?
- **Modüler yapı**: İhtiyacın olan bileşenleri seçip ekleyerek hafif ve odaklı projeler oluşturabilirsin.
- **Clean Architecture ve SOLID uyumu**: Katmanlar arası bağımlılıklar net şekilde ayrılır, test edilebilirlik artar.
- **Opsiyonel altyapı servisleri**: Logging, repository, unit of work, rate limiting, performance, validation ve daha fazlası tek bir paket eklenerek etkinleştirilebilir.

## Hangi Senaryolarda Kullanılır?
- ASP.NET Core tabanlı modern mikro servis veya monolit projelerde, altyapıyı hızla ayağa kaldırmak için.
- Entity auditing, soft delete ve repository pattern'ini standartlaştırmak isteyen ekipler için.
- Merkezi loglama, performans izleme ve hata yönetimini minimum kodla entegre etmek isteyenler için.

## Ek Kaynaklar
- Örnek kodlar ve detaylı kurulum adımları için [Kullanım Kılavuzu](OKS_USAGE.md).
- Çatıya genel bakış ve proje kapsamı için [README](README.md).
