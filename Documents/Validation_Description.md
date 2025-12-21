# Validation (FluentValidation) - Description

OKS validation katmanı, FluentValidation validator'larını otomatik DI'a ekler ve action başlamadan önce `OksValidationFilter` ile doğrular. `[OksSkipValidation]` attribute'u ile esnek şekilde devre dışı bırakılabilir.

## Başlıca bileşenler
- **Oks.Web.Validation**: FluentValidation entegrasyonu ve filtre.
- **Oks.Web.Abstractions**: Ortak attribute sözleşmeleri.

## Özellikler
- `AddOksFluentValidation(typeof(AssemblyMarker))` çağrısıyla assembly içindeki tüm validator'lar taranır.
- Hata mesajları `Result` sarmalayıcısı ile tutarlı biçimde döner (Result wrapping eklentisiyle birlikte).
- Belirli action'larda attribute ile validation'ı açıp kapatma imkânı.

---
## Usage

Kurulum ve kopyala-yapıştır kod örnekleri için: [Validation_Usage.md](Validation_Usage.md)
