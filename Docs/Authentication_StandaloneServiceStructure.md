# Authentication - Standalone Service Structure

[Ana sayfa](../README.md) | [Authentication - Description](Authentication_Description.md) | [Authentication - Usage](Authentication_Usage.md) | [Authentication - Project Structure](Authentication_ProjectStructure.md)

Bu dokuman, Authentication alanini ayri deploy edilen servis olarak kurmak isteyen projeler icin hazirlanmistir. Ornek isimlendirme `Authr` uzerindendir.

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
        Entities/
        Events/
        ValueObjects/
      Roles/
        Entities/
        Events/
        ValueObjects/
      Clients/
        Entities/
        Events/
        ValueObjects/
      RefreshTokens/
        Entities/
        Events/
        ValueObjects/
    Events/
    Enums/
    Policies/
    Repositories/
    ValueObjects/

  Authr.Application/
    Authr.Application.csproj
    Common/
      Exceptions/
      Models/
      Behaviors/
    Auth/
      Services/
        AuthenticationOrchestratorService.cs
        IAuthenticationOrchestratorService.cs
        LoginAppService.cs
        ILoginAppService.cs
        LoginDomainService.cs
        ILoginDomainService.cs
        RefreshTokenAppService.cs
        IRefreshTokenAppService.cs
        RefreshTokenDomainService.cs
        IRefreshTokenDomainService.cs
      Commands/
        Login/
          LoginCommand.cs
          LoginCommandHandler.cs
          LoginRequestDto.cs
          LoginResponseDto.cs
          LoginCommandValidator.cs
        RefreshToken/
          RefreshTokenCommand.cs
          RefreshTokenCommandHandler.cs
          RefreshTokenRequestDto.cs
          RefreshTokenResponseDto.cs
          RefreshTokenCommandValidator.cs
        Logout/
          LogoutCommand.cs
          LogoutCommandHandler.cs
          LogoutRequestDto.cs
          LogoutResponseDto.cs
          LogoutCommandValidator.cs
        RevokeToken/
          RevokeTokenCommand.cs
          RevokeTokenCommandHandler.cs
          RevokeTokenRequestDto.cs
          RevokeTokenResponseDto.cs
          RevokeTokenCommandValidator.cs
      Queries/
        IntrospectToken/
          IntrospectTokenQuery.cs
          IntrospectTokenQueryHandler.cs
          IntrospectTokenRequestDto.cs
          IntrospectTokenResponseDto.cs
          IntrospectTokenQueryValidator.cs
        GetSessionById/
          GetSessionByIdQuery.cs
          GetSessionByIdQueryHandler.cs
          GetSessionByIdRequestDto.cs
          GetSessionByIdResponseDto.cs
          GetSessionByIdQueryValidator.cs
    Users/
      Services/
        UserPermissionResolverService.cs
        IUserPermissionResolverService.cs
        RegisterUserAppService.cs
        IRegisterUserAppService.cs
        RegisterUserDomainService.cs
        IRegisterUserDomainService.cs
      Commands/
        RegisterUser/
          RegisterUserCommand.cs
          RegisterUserCommandHandler.cs
          RegisterUserRequestDto.cs
          RegisterUserResponseDto.cs
          RegisterUserCommandValidator.cs
      Queries/
        GetUserById/
          GetUserByIdQuery.cs
          GetUserByIdQueryHandler.cs
          GetUserByIdRequestDto.cs
          GetUserByIdResponseDto.cs
          GetUserByIdQueryValidator.cs
    Roles/
      Services/
        RolePermissionMapperService.cs
        IRolePermissionMapperService.cs
        CreateRoleAppService.cs
        ICreateRoleAppService.cs
        CreateRoleDomainService.cs
        ICreateRoleDomainService.cs
      Commands/
        CreateRole/
          CreateRoleCommand.cs
          CreateRoleCommandHandler.cs
          CreateRoleRequestDto.cs
          CreateRoleResponseDto.cs
          CreateRoleCommandValidator.cs
      Queries/
        GetRoleById/
          GetRoleByIdQuery.cs
          GetRoleByIdQueryHandler.cs
          GetRoleByIdRequestDto.cs
          GetRoleByIdResponseDto.cs
          GetRoleByIdQueryValidator.cs
    Clients/
      Services/
        ClientSecretService.cs
        IClientSecretService.cs
        CreateClientAppService.cs
        ICreateClientAppService.cs
        CreateClientDomainService.cs
        ICreateClientDomainService.cs
      Commands/
        CreateClient/
          CreateClientCommand.cs
          CreateClientCommandHandler.cs
          CreateClientRequestDto.cs
          CreateClientResponseDto.cs
          CreateClientCommandValidator.cs
      Queries/
        GetClientById/
          GetClientByIdQuery.cs
          GetClientByIdQueryHandler.cs
          GetClientByIdRequestDto.cs
          GetClientByIdResponseDto.cs
          GetClientByIdQueryValidator.cs

  Authr.Infrastructure/
    Authr.Infrastructure.csproj
    Persistence/
      Context/
      Configurations/
      Repositories/
      UnitOfWork/
      Migrations/
      Seed/
    Security/
      Jwt/
      Hashing/
    Integrations/
      Cache/
      Messaging/
      Logging/
      Time/
      Observability/
    DependencyInjection/

  Authr.Contracts/
    Authr.Contracts.csproj
    Requests/
    Responses/
    Events/

  Authr.API/
    Authr.API.csproj
    Controllers/
    Middleware/
    Extensions/
    OpenApi/
    HealthChecks/
    Configuration/
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

## Application klasorleme standardi

Bu servis icin `Application` katmaninda standart su olmalidir:

- Aggregate altinda `Services/`, `Commands/`, `Queries/` klasorleri bulunur
- Her command/query kendi alt klasorunde tutulur
- O alt klasorde su dosyalar birlikte yer alir:
  - `Command` veya `Query`
  - `Handler`
  - `RequestDto`
  - `ResponseDto`
  - `Validator`
- Entity icinde constructor ve method bulunmaz; davranislar ilgili aggregate altindaki `Services/` klasorune tasinir
- `Services/` altinda:
  - `DomainService`: is kuralini uygular
  - `AppService`: use-case akislarini ve orkestrasyonu yonetir

Ornek ayrim:

```csharp
public async Task AddFriendAsync(Guid userId, Guid friendUserId)
{
    var user = await _repo.GetByIdAsync(userId);
    var friend = await _repo.GetByIdAsync(friendUserId);

    _domainService.AddFriend(user, friend);

    await _repo.UpdateAsync(user);
}
```

Bu `AppService` akisinda soyledigi sey:

- veriyi al
- domain kuralini calistir
- sonucu kaydet

```csharp
public void AddFriend(User user, User friend)
{
    if (user.Id == friend.Id)
        throw new InvalidOperationException(...);

    if (!friend.IsActive)
        throw new InvalidOperationException(...);

    if (user.HasFriend(friend.Id))
        throw new InvalidOperationException(...);

    user.AddFriendInternal(friend.Id);
}
```

Bu `DomainService` tarafinda soyledigi sey:

- bu isin kurali budur
- hangi durumda izin var / hangi durumda yok

Ornek:

```text
Aggregates/
  Users/
    Services/
      UserPermissionResolverService.cs
      IUserPermissionResolverService.cs
    Commands/
      RegisterUser/
        RegisterUserCommand.cs
        RegisterUserCommandHandler.cs
        RegisterUserRequestDto.cs
        RegisterUserResponseDto.cs
        RegisterUserCommandValidator.cs
```

Bu ayni mantik diger katmanlarda da surdurulmelidir:

- `Domain`: aggregate altinda `Entities`, `Events`, `ValueObjects`
- `Infrastructure`: `Persistence`, `Security`, `Integrations`, `DependencyInjection`
- `API`: `Controllers`, `Middleware`, `OpenApi`, `HealthChecks`, `Configuration`
- `Contracts`: `Requests`, `Responses`, `Events`

Tek tek sinif isimleri siralamak yerine, sorumluluk bazli klasorler gosterilmelidir. OksFramework'te zaten var olan ortak altyapi dosyalari ornek diye tekrar sayilmamalidir.

## Domain model notu

Bu dokumandaki yaklasimda `Domain` entity'leri anemic model olarak ele alinir:

- entity icinde constructor bulunmaz
- entity icinde davranis metodu bulunmaz
- veri tasiyan domain tipleri `Domain` katmaninda kalir
- is kurali `DomainService` ile uygulanir
- use-case orkestrasyonu `AppService` ile yonetilir

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
