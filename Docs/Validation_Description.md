# Validation (FluentValidation) - Description

[Ana sayfa](../README.md)

OKS validation katmanı, FluentValidation validator'larını otomatik DI'a ekler ve doğrulamayı MVC + Minimal API + MediatR için ortaklaştırır.

## Önemli not
- `OksValidation` attribute'una artık ihtiyaç yoktur ve kaldırılmıştır.
- Validation davranışı, validator varsa otomatik çalışır.
- `[OksSkipValidation]` ile endpoint/request bazında validation kapatılabilir.

## Bileşenler
- `OksValidationFilter` (MVC)
- `OksMinimalApiValidationFilter` (Minimal API endpoint filter)
- `OksValidationBehavior<TRequest,TResponse>` (MediatR pipeline)

## Davranış özeti
- İlgili tipe ait `IValidator<T>` yoksa akış kesilmez.
- `[OksSkipValidation]` varsa validation atlanır.
- Hata varsa 400 + OKS result formatı döner (MediatR response tipi uygun değilse `ValidationException`).

---
## Usage

Kurulum ve örnekler için: [Validation_Usage.md](Validation_Usage.md)
