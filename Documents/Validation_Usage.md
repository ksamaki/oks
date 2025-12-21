# Validation Modülü Kullanım Kılavuzu

Bu rehber, OKS Validation modülünü kullanarak FluentValidation kurallarını API pipeline'ına nasıl ekleyeceğini anlatır.

> Navigasyon: [Açıklama](Validation_Description.md) | [README](README.md)

## Kurulum
1. `Oks.Web.Validation` paketini projene ekle.
2. `builder.Services.AddControllers().AddOksFluentValidation(typeof(Program).Assembly);` çağrısını ekle.
3. Gerekirse `AddOksResultWrapping` ile hata formatını standartlaştır.

## Kullanım
- `AbstractValidator<TRequest>` sınıfları oluşturarak kurallarını tanımla.
- Controller aksiyonlarınızdaki parametreler için otomatik doğrulama devreye girer.
- Hatalar, OKS sonuç modeli formatında döner.

## İpuçları
- Validasyon kurallarını modüllere ayırarak bakımını kolaylaştır.
- Performans için gereksiz ağır doğrulamalardan kaçın.
