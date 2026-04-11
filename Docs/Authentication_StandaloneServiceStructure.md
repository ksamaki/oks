# Authentication - Standalone Service Structure

[Ana sayfa](../README.md) | [Authentication - Description](Authentication_Description.md) | [Authentication - Usage](Authentication_Usage.md) | [Authentication - Project Structure](Authentication_ProjectStructure.md)

Bu dokuman, Authentication alanini mevcut bir uygulamanin ic modulu olarak degil, ayri deploy edilen bir servis olarak kurmak isteyen projeler icin hazirlanmistir. Ornek isimlendirme `Authr` uzerindendir.

Bu yapi su senaryoya hitap eder:

- `Authr` ayri repository veya ayri solution olabilir
- ayri container olarak calisir
- ayri database kullanir
- diger servisler JWT validation veya introspection ile buna baglanir
- ileride OpenIddict / OAuth server / SSO ihtiyacina dogru buyutulebilir

## Onerilen ust seviye yapi

```text
Authr/
  README.md
  docker-compose.yml
  .env
  docs/
    architecture.md
    deployment.md
    api-contracts.md
  src/
    Authr.slnx
    Authr.Domain/
    Authr.Application/
    Authr.Infrastructure/
    Authr.API/
    Authr.Contracts/
  tests/
    Authr.Domain.Tests/
    Authr.Application.Tests/
    Authr.Infrastructure.Tests/
    Authr.API.Tests/
    Authr.IntegrationTests/
  deploy/
    docker/
      Authr.API.Dockerfile
    k8s/
      authr-deployment.yaml
      authr-service.yaml
      authr-configmap.yaml
      authr-secret.example.yaml
```

## Library bazli yapi

```text
src/
  Authr.Domain/
    Authr.Domain.csproj
    Aggregates/
      Users/
        User.cs
        UserSession.cs
      Roles/
        Role.cs
      Clients/
        Client.cs
      RefreshTokens/
        RefreshToken.cs
    Events/
      UserLoggedInDomainEvent.cs
      UserLoggedOutDomainEvent.cs
      RefreshTokenRevokedDomainEvent.cs
    Enums/
      ClientType.cs
      UserStatus.cs
      TokenStatus.cs
    Policies/
      PasswordPolicy.cs
      LockoutPolicy.cs
    Repositories/
      IUserRepository.cs
      IClientRepository.cs
      IRefreshTokenRepository.cs
    ValueObjects/
      EmailAddress.cs
      PasswordHash.cs
      RefreshTokenHash.cs

  Authr.Application/
    Authr.Application.csproj
    Common/
      Exceptions/
      Models/
      Behaviors/
    Auth/
      Commands/
        Login/
        RefreshToken/
        Logout/
        RevokeToken/
      Queries/
        IntrospectToken/
        GetSessionById/
      Dtos/
      Validators/
      Interfaces/
      Services/
    Users/
      Commands/
      Queries/
      Dtos/
      Validators/
      Interfaces/
      Services/
    Roles/
      Commands/
      Queries/
      Dtos/
      Validators/
      Interfaces/
      Services/
    Clients/
      Commands/
      Queries/
      Dtos/
      Validators/
      Interfaces/
      Services/

  Authr.Infrastructure/
    Authr.Infrastructure.csproj
    Persistence/
      AuthrDbContext.cs
      Configurations/
      Repositories/
      UnitOfWork/
      Migrations/
      Seed/
    Security/
      Jwt/
        JwtTokenGenerator.cs
        JwtSigningKeyProvider.cs
      Hashing/
        PasswordHasher.cs
        SecretHasher.cs
    Integrations/
      Cache/
      Messaging/
      Logging/
      Time/
    DependencyInjection/
      ServiceCollectionExtensions.cs

  Authr.Contracts/
    Authr.Contracts.csproj
    Requests/
      LoginRequest.cs
      RefreshTokenRequest.cs
    Responses/
      LoginResponse.cs
      RefreshTokenResponse.cs
      IntrospectTokenResponse.cs

  Authr.API/
    Authr.API.csproj
    Controllers/
      AuthController.cs
      UsersController.cs
      ClientsController.cs
      HealthController.cs
    Middleware/
      ExceptionHandlingMiddleware.cs
      CorrelationMiddleware.cs
      TenantResolutionMiddleware.cs
    Extensions/
      ServiceCollectionExtensions.cs
      ApplicationBuilderExtensions.cs
    OpenApi/
      SwaggerConfiguration.cs
    Program.cs
    appsettings.json
    appsettings.Development.json
```

## Servis sinirlari

`Authr` servisinde yalnizca authentication ve authorization ile ilgili sorumluluklar bulunmalidir:

- login
- refresh token
- logout / revoke
- client management
- role / permission management
- token introspection
- session management
- auth audit

Su alanlari bu servise koymamak daha dogru olur:

- kullanicinin is domain profili
- siparis, mesaj, arkadaslik, odeme gibi is modulleri
- ana uygulamanin genel user profile CRUD ekranlari

Yani `Authr.User`, sistem kullanicisi ve kimlik/erisim baglaminda ele alinmalidir; uygulama profili ile karistirilmamalidir.

## API siniri ve gateway notu

Authentication ayri servis oldugunda tipik akis sunlardir:

1. Client `Authr.API` uzerinden login olur.
2. `Authr` access token + refresh token uretir.
3. Diger servisler JWT bearer validation yapar.
4. Gerekirse admin veya internal servisler `introspect` endpoint'ini cagirir.

Onerilen endpoint gruplari:

- `/api/auth/login`
- `/api/auth/refresh`
- `/api/auth/logout`
- `/api/auth/introspect`
- `/api/users`
- `/api/roles`
- `/api/clients`
- `/health`

API gateway kullaniyorsaniz:

- login/refresh endpoint'leri gateway arkasinda yayinlanabilir
- internal admin endpoint'leri network policy ile sinirlanabilir
- introspection endpoint'i sadece internal network'e acilabilir

## Database ayrimi

Authentication servisinin ayri DB kullanmasi kuvvetle onerilir.

Onerilen DB kapsami:

- users
- roles
- permissions
- clients
- refresh tokens
- sessions
- login attempts
- security events

Bu DB'yi diger uygulama DB'leri ile join etmeyin. Gerekli veri paylasimi icin:

- event publish
- projection
- cache
- internal API

kullanin.

## OKS ile entegrasyon

Bu standalone yapida OKS su sekilde kullanilabilir:

- `Authr.Infrastructure`
  - `AddOksEfCore<AuthrDbContext>()`
  - `AddOksRepositoryLogging()`
  - `AddOksCaching()` gerekiyorsa

- `Authr.API`
  - `AddOksCurrentUserProvider()`
  - `AddOksValidation()`
  - `AddOksUnitOfWork()`
  - `AddOksResultWrapping()`
  - `UseOksExceptionHandling()` benzeri mevcut exception pipeline

Sinir ayni kalmali:

- `Domain` OKS bilmez
- `Application` mumkunse yalnizca abstraction bilir
- `Infrastructure` ve `API` compose eder

## Ayrica eklenmesi gereken operational klasorler

Standalone servis oldugu icin uygulama iskeletine su alanlar da eklenmeli:

- `deploy/docker/`
- `deploy/k8s/`
- `docs/deployment.md`
- `docs/api-contracts.md`
- `tests/Authr.IntegrationTests/`
- `src/Authr.API/HealthChecks/`
- `src/Authr.Infrastructure/Observability/`

## Observability ve security

Bu servis icin su basliklar kritik:

- structured logging
- auth audit log
- failed login metrics
- refresh token reuse alert
- brute force / lockout handling
- correlation id
- secret rotation
- signing key rotation
- rate limit

Bu nedenle su dosyalar yararli olur:

```text
src/
  Authr.Infrastructure/
    Observability/
      AuthMetrics.cs
      AuthAuditLogWriter.cs
      SecurityAlertPublisher.cs
  Authr.API/
    HealthChecks/
      DatabaseHealthCheck.cs
      SigningKeyHealthCheck.cs
```

## Ayrica onerilen teknik basliklar

- `Outbox`
Kullanici kilitlendi, role degisti, client kapatildi gibi olaylar diger servislere aktarilacaksa gereklidir.

- `KeyManagement`
JWT signing key yasam dongusu acikca ayrilmalidir.

- `BackgroundJobs`
Expired session temizligi, refresh token cleanup, login attempt retention gibi isler icin gerekir.

- `OpenApi/Versioning`
External ve internal endpoint'leri zamanla ayirmak icin ise yarar.

## Minimum ilk surum

Ilk cikista asagidaki kapsam yeterlidir:

1. `Authr.Domain`
2. `Authr.Application`
3. `Authr.Infrastructure`
4. `Authr.Contracts`
5. `Authr.API`
6. `AuthrDbContext`
7. `Login`, `RefreshToken`, `Logout`, `IntrospectToken`
8. `User`, `Role`, `Client`, `RefreshToken`
9. repository logging + validation + unit of work
10. health check + dockerfile + integration test fixture

Bu nokta, ayri servis olarak canliya cikabilecek ilk makul tabandir.
