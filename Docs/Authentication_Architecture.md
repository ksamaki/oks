# Oks Authentication Altyapısı – Teknik Tasarım

[Ana sayfa](../README.md) | [Authentication - Description](Authentication_Description.md) | [Authentication - Usage](Authentication_Usage.md)

Bu doküman, OksFramework içinde kullanılacak modüler authentication altyapısının **gerçek geliştirmeye başlanabilir** ilk iskelet tasarımını içerir.

## 1) Önerilen Proje Yapısı

- `Oks.Authentication.Abstractions`
  - DTO, contract, option ve extension noktaları
- `Oks.Authentication.Core`
  - Use-case orchestration (`login`, `refresh`, `logout/revoke`) ve güvenlik olay akışı
- `Oks.Authentication.Jwt`
  - Varsayılan JWT access token üretimi
- `Oks.Authentication.OpenIddict` (opsiyonel)
  - OpenID Connect / OpenIddict adapter katmanı
- `Oks.Authentication.EntityFrameworkCore`
  - Entity’ler, `DbContext`, fluent config, seed, migration helper
- `Oks.Authentication.AspNetCore`
  - `IServiceCollection`/`IApplicationBuilder` extension’ları ve policy handler

## 2) Katman Sorumlulukları

- **Abstractions**: Tüm modüllerin konuştuğu sözleşmeler (`IAuthenticationService`, `ITokenIssuer`, `IRefreshTokenStore` vb.).
- **Core**: İş akışını yönetir; dış bağımlılıkları interface ile çağırır. OpenIddict’e direkt bağlı değildir.
- **Jwt**: `ITokenIssuer` implementasyonu ile JWT üretir.
- **OpenIddict (optional)**: Sadece ihtiyaç halinde eklenir; Core/Jwt kullanan uygulamaları etkilemez.
- **EFCore**: Kalıcılık, tablo şeması, seed, auto-migrate.
- **AspNetCore**: Kolay kurulum API’si (`AddOksAuthentication`, `UseOksAuthentication`) + permission policy.

> Abstraction-only notu: `AddOksAuthentication` yalnızca ASP.NET Core auth/authorization pipeline’ını kurar; `IAuthenticationService` ve diğer domain sözleşmeleri host tarafından explicit compose edilir (`AddOksAuthenticationCore`, `AddOksJwt`, `AddOksAuthenticationEntityFramework`, custom store implementasyonları).

## 3) Entity Listesi ve İlişkiler

Temel tablolar:
- `OksUser`
- `OksRole`
- `OksPermission`
- `OksRolePermission`
- `OksUserRole`
- `OksUserClaim`
- `OksClient`
- `OksRefreshToken`
- `OksUserSession`
- `OksLoginAttempt`
- `OksSecurityEvent` (opsiyonel ama önerilir)

İlişkiler:
- `User -> UserRole -> Role`
- `Role -> RolePermission -> Permission`
- `User -> UserClaim`
- `Session -> RefreshToken`

## 4) DbContext ve Entity Configuration

- `OksAuthenticationDbContext` içinde tüm DbSet’ler tanımlı.
- `ApplyConfigurationsFromAssembly` ile konfigürasyonlar otomatik eklenir.
- `Schema` adı `OksAuthenticationEfCoreOptions.Schema` üzerinden özelleştirilebilir.
- Unique index’ler tenant-aware olacak şekilde kurgulanmıştır (`TenantId + BusinessKey`).

## 5) Kurulum Extension Method Örnekleri

```csharp
services
    .AddOksAuthentication()
    .AddOksAuthenticationCore()
    .AddOksJwt(options =>
    {
        options.Issuer = "oks-auth";
        options.Audience = "oks-services";
        options.SigningKey = configuration["Auth:Jwt:SigningKey"]!;
    })
    .AddOksAuthenticationEntityFramework<OksAuthenticationDbContext>(options =>
    {
        options.Schema = "oks_auth";
        options.AutoMigrate = true;
    });

// optional
services.AddOksOpenIddict<MyOpenIddictConfigurator>(opt => { /* grant/scope */ });

app.UseOksAuthentication();
```

## 6) JWT Kullanım Akışı

1. Username/password + client bilgisi gelir.
2. Client active mi ve secret doğru mu kontrol edilir.
3. Kullanıcı doğrulanır.
4. Minimal claim set ile kısa ömürlü JWT üretilir (`sub`, `oks_uid`, `oks_cid`, `oks_sid`, role/permission).
5. Refresh token random üretilir, **hashlenerek** DB’ye yazılır.

## 7) Refresh Token Akışı

1. Client `refresh_token` gönderir.
2. Token hashlenir, DB’de aranır.
3. Revoked/consumed/expired kontrol edilir.
4. Rotation: eski token revoked/consumed işaretlenir, yeni refresh token oluşturulur.
5. Revoke reason ve zaman bilgisi tutulur.

## 8) OpenIddict Opsiyonel Entegrasyon Yaklaşımı

- `Oks.Authentication.OpenIddict` modülü adapter yaklaşımı kullanır.
- Core modül OpenIddict paketini bilmez.
- Host uygulama OpenIddict referansını kendi isterse ekler ve `IOksOpenIddictConfigurator` implement eder.
- Bu sayede JWT-only senaryoda OpenIddict bağımlılığı yoktur.

## 9) Otomatik Migration / Tablo Oluşturma

- `UseOksAuthenticationEntityFrameworkAsync` ile startup’ta opsiyonel `Database.MigrateAsync()` çağrısı yapılabilir.
- Minimum kurulum için host uygulama sadece DbContext + extension çağrısı yapar.
- Gelişmiş kullanımda host kendi DbContext’ini `OksAuthenticationDbContext`’ten türeterek genişletebilir.

## 10) Seed Yaklaşımı

- `OksAuthenticationSeeder` idempotent şekilde çalışır.
- Tekrar çalıştırmada aynı role/permission/client tekrar eklenmez.
- Seed ile örnek olarak:
  - `Admin` role
  - temel auth permission’ları
  - `oks_admin_web` ve `oks_internal_svc` client kayıtları

## 11) Örnek Interface ve Class İskeletleri

Öne çıkanlar:
- `IAuthenticationService`
- `ITokenIssuer`
- `IRefreshTokenStore`
- `IClientStore`
- `IUserCredentialValidator`
- `ISecretHasher`
- `IAuthSecurityEventPublisher`
- `DefaultAuthenticationService`
- `JwtTokenIssuer`
- `EfCoreRefreshTokenStore`

## 12) Tasarım Kararları ve Kısa Gerekçeler

- **Core’da OpenIddict bağımlılığı yok**: Opsiyonel modül prensibi korunur.
- **JWT minimal claim**: Token boyutu ve veri sızıntı riski düşer.
- **Refresh token hashli saklama**: DB sızıntısında ham token ifşa olmaz.
- **Rotation + revoke**: Token replay riskini azaltır.
- **TenantId hazırlığı**: Tam multi-tenant yok ama şema/index tasarımı hazır.
- **Hook noktaları**: Security event publisher ve store abstraction’ları ile gelecekte Redis/event-bus entegrasyonu kolay.

---

## Mikroservis Notları

- Auth server ayrı servis olarak koşabilir.
- Diğer servisler sadece JWT doğrulama yaparak bağımsız ölçeklenir.
- Service-to-service için `client_credentials` client tipi ve scope modeli desteklenir.
- Ortak claim sözlüğü (`OksClaimTypes`) servisler arası standardizasyon sağlar.
