# Validation Modülü Açıklaması

OKS Validation modülü, FluentValidation tabanlı doğrulamayı ASP.NET Core pipeline'ına entegre ederek controller eylemleri çalışmadan önce giriş verisinin doğrulanmasını sağlar.

> Navigasyon: [Kullanım Kılavuzu](Validation_Usage.md) | [README](README.md)

## Sağladıkları
- Controller aksiyonlarından önce otomatik doğrulama.
- Hata sonuçlarını OKS sonuç modeliyle tutarlı biçimde döndürür.
- FluentValidation kurallarını assembly bazında hızlıca kaydeder.

## Ne Zaman Kullanmalı?
- API isteklerinin erken aşamada kontrol edilmesini istediğinde.
- Hata mesajlarını standartlaştırmak ve sarılı sonuç modeliyle dönmek istediğinde.
