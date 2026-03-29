# Authentication - Description

[Ana sayfa](../README.md)

OKS Authentication modülü; **JWT varsayılan**, **OpenIddict opsiyonel**, **multi-client destekli**, **mikroservis uyumlu** ve **EF Core ile kendi tablolarını yönetebilen** bir auth altyapısı sağlar.

## Modüler yapı

| Modül | Sorumluluk | Zorunlu mu? |
|---|---|---|
| `Oks.Authentication.Abstractions` | Kontratlar, DTO modelleri, option tipleri | ✅ |
| `Oks.Authentication.Core` | Login/refresh/logout orkestrasyonu | ✅ |
| `Oks.Authentication.Jwt` | JWT üretimi ve JWT ayarları | ✅ (default kullanım) |
| `Oks.Authentication.EntityFrameworkCore` | Entity/DbContext/config/seed/migrate | ✅ (DB tabanlı kullanımda) |
| `Oks.Authentication.AspNetCore` | DI + middleware + authorization policy | ✅ |
| `Oks.Authentication.OpenIddict` | OpenID Connect / Authorization Server adapter | ❌ (opsiyonel) |

> Not: `Core` modülü OpenIddict'e doğrudan bağımlı değildir. Sadece JWT kullanacak host uygulamalar OpenIddict referansı almak zorunda kalmaz.

## Desteklenen temel senaryolar

- Username/password login
- Access token üretimi
- Refresh token üretimi + yenileme (rotation)
- Logout / revoke
- Client bazlı erişim kontrolü
- Role + Permission bazlı authorization
- SPA, Mobile, Admin, Internal Service, External Integration client tipleri

## Token yaklaşımı

### Access token (JWT)
- Kısa ömürlü signed JWT
- Minimum claim set:
  - `sub`
  - `oks_uid`
  - `oks_cid`
  - `oks_sid`
  - (varsa) `oks_tid`
  - role ve permission claim'leri
- Hassas kullanıcı verisi token içine yazılmaz

### Refresh token
- Rastgele üretilir
- DB'de **hashlenmiş** saklanır
- Rotation desteklenir
- Revoke/expire bilgisi tutulur
- Kullanım durumu izlenir (`RevokedAtUtc`, `ConsumedAtUtc`, `ReplacedByTokenHash`)

## Multi-client modeli

`OksClient` için beklenen alanlar:
- Name
- ClientId
- Secret (hashlenmiş tutulabilir)
- ClientType
- AllowedGrantTypes
- AllowedScopes
- RedirectUris
- IsActive

Örnek client tipleri:
- `spa_web`
- `mobile_app`
- `admin_panel`
- `internal_service`
- `external_integration`

## Mikroservis uyumluluğu

- Auth server ayrı bir servis olarak konumlanabilir
- Diğer servisler yalnızca JWT doğrulama yaparak çalışabilir
- Service-to-service (`client_credentials`) desteğine uygun model
- Ortak claim sözlüğü (`OksClaimTypes`) ile servisler arası standardizasyon
- Gelecekte Redis/distributed cache için abstraction noktaları
- Gelecekte audit/security event publish için hook noktaları (`IAuthSecurityEventPublisher`)

## EF Core tablo seti

`Oks.Authentication.EntityFrameworkCore` modülü aşağıdaki tablo modelini içerir:

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
- `OksSecurityEvent`

İlişki modeli:
- `User -> UserRole -> Role`
- `Role -> RolePermission -> Permission`

## Otomatik migration ve schema

- `UseOksAuthenticationEntityFrameworkAsync` ile opsiyonel auto-migrate
- Schema adı `OksAuthenticationEfCoreOptions.Schema` ile özelleştirilebilir
- `ApplyConfigurationsFromAssembly` ile entity config'ler otomatik uygulanır

## Seed yaklaşımı

`OksAuthenticationSeeder` idempotent tasarlanmıştır:
- Varsayılan `Admin` role
- Temel permission setleri
- `oks_admin_web` client
- `oks_internal_svc` client

Aynı kayıt ikinci kez eklenmez.

## Multi-tenant hazırlığı

Tam multi-tenant implementasyonu bu aşamada yoktur; ancak altyapı hazırlanmıştır:
- Kritik entity'lerde opsiyonel `TenantId`
- Tenant-aware unique index yaklaşımı
- Repository/store sözleşmeleri tenant-aware genişletmeye uygun

---
## Usage

Detaylı kurulum ve örnekler için: [Authentication_Usage.md](Authentication_Usage.md)
