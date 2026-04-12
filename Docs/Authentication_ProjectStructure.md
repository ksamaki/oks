# Authentication - Project Structure

[Ana sayfa](../README.md) | [Authentication - Description](Authentication_Description.md) | [Authentication - Usage](Authentication_Usage.md) | [Authentication - Architecture](Authentication_Architecture.md)

Bu dokuman, yeni bir projede `Auth` / `Authr` bounded context'ini CQRS, DDD ve AggregateRoot odakli bir klasorleme ile baslatmak icin ornek bir yapi sunar. Hedef; OKS'nin repository, unit of work, validation, logging, caching ve web pipeline yetenekleriyle uyumlu, buyudukce bozulmayan bir iskelet vermektir.

> Mimari not: `Auth` / `Authr`, ihtiyaca gore ayni container icinde modul olarak da ayri container/ayri deployment olarak da kullanilabilir. OKS tarafinda bu esneklik; ayri `DbContext`, ayri migration seti, ayri deployment ve gerekirse ayri cache/policy konfigrasyonu ile desteklenir.

## Hedeflenen katmanlar

- `Domain`: AggregateRoot, entity, value object, enum, domain event, domain service, repository kontratlari.
- `Application`: Use-case katmani. CQRS handler'lari, command/query modelleri, DTO'lar, validator'lar, application servisleri ve port'lar.
- `Infrastructure`: EF Core, persistence, external adapter'lar, cache, messaging, token provider, password hasher, audit/log entegrasyonlari.
- `API`: Controller, middleware, filter, endpoint composition, auth pipeline, request/response surface.

## Onerilen solution yapisi

```text
src/
  Authr.slnx
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
    Common/
    Enums/
    ValueObjects/
    Events/
    Services/
    Repositories/

  Authr.Application/
    Authr.Application.csproj
    Abstractions/
      Clock/
        IDateTimeProvider.cs
      Security/
        IJwtTokenGenerator.cs
        ISecretHasher.cs
        ICurrentClientProvider.cs
      Persistence/
        IAuthUnitOfWork.cs
      Messaging/
        IEventPublisher.cs
    Behaviors/
      ValidationBehavior.cs
      LoggingBehavior.cs
      TransactionBehavior.cs
    Aggregates/
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
          ChangePassword/
            ChangePasswordCommand.cs
            ChangePasswordCommandHandler.cs
            ChangePasswordRequestDto.cs
            ChangePasswordResponseDto.cs
            ChangePasswordCommandValidator.cs
          LockUser/
            LockUserCommand.cs
            LockUserCommandHandler.cs
            LockUserRequestDto.cs
            LockUserResponseDto.cs
            LockUserCommandValidator.cs
        Queries/
          GetUserById/
            GetUserByIdQuery.cs
            GetUserByIdQueryHandler.cs
            GetUserByIdRequestDto.cs
            GetUserByIdResponseDto.cs
            GetUserByIdQueryValidator.cs
          GetUserPermissions/
            GetUserPermissionsQuery.cs
            GetUserPermissionsQueryHandler.cs
            GetUserPermissionsRequestDto.cs
            GetUserPermissionsResponseDto.cs
            GetUserPermissionsQueryValidator.cs
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
      Sessions/
        Services/
          RevokeSessionAppService.cs
          IRevokeSessionAppService.cs
          RevokeSessionDomainService.cs
          IRevokeSessionDomainService.cs
        Commands/
          RevokeSession/
            RevokeSessionCommand.cs
            RevokeSessionCommandHandler.cs
            RevokeSessionRequestDto.cs
            RevokeSessionResponseDto.cs
            RevokeSessionCommandValidator.cs
        Queries/
          GetActiveSessions/
            GetActiveSessionsQuery.cs
            GetActiveSessionsQueryHandler.cs
            GetActiveSessionsRequestDto.cs
            GetActiveSessionsResponseDto.cs
            GetActiveSessionsQueryValidator.cs
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
        Queries/
          IntrospectToken/
            IntrospectTokenQuery.cs
            IntrospectTokenQueryHandler.cs
            IntrospectTokenRequestDto.cs
            IntrospectTokenResponseDto.cs
            IntrospectTokenQueryValidator.cs

  Authr.Infrastructure/
    Authr.Infrastructure.csproj
    Persistence/
      Context/
      Configurations/
      Repositories/
      Migrations/
      Seed/
      UnitOfWork/
    Identity/
    Integrations/
      Caching/
      Logging/
      Messaging/
      Time/
      Security/
    DependencyInjection/

  Authr.API/
    Authr.API.csproj
    Controllers/
    Middleware/
    Filters/
    Extensions/
    Contracts/
      Requests/
      Responses/
    OpenApi/
    HealthChecks/
    Configuration/

tests/
  Authr.Domain.Tests/
  Authr.Application.Tests/
  Authr.Infrastructure.Tests/
  Authr.API.Tests/
```

## Katman bazli kurallar

### 1) Domain

- AggregateRoot'lar burada yasar. `User`, `Role`, `Client` gibi kok nesneler kendi invariant'larini korur.
- `Domain` katmani `Infrastructure` veya `API` bilmez.
- `IReadRepository` / `IWriteRepository` yerine, aggregate odakli repository kontratlari tanimlamak daha temiz olur.
- Enum, value object ve domain event'ler dogrudan aggregate davranisini desteklemelidir.
- Login gibi saf orkestrasyon use-case'leri burada degil `Application` katmaninda olmalidir.
- OksFramework'te zaten var olan ortak base tipler veya altyapi kontratlari, burada gereksiz ornek dosya listesi olarak tekrar edilmemelidir.
- Bu dokumandaki yaklasimda `Domain` entity'leri icinde constructor ve davranis metotlari bulunmaz.
- Entity davranislari `Application` katmaninda ilgili aggregate altindaki `Services/` klasorune tasinmalidir.

### 2) Application

- CQRS handler'lari burada olur.
- Her AggregateRoot icin ayri klasor acmak, use-case'leri is alanina gore toplar.
- `Commands` ve `Queries` altinda her use-case klasoru kendi `Command/Query`, `Handler`, `RequestDto`, `ResponseDto`, `Validator` dosyalarini birlikte tutmalidir.
- Aggregate seviyesinde tekrar kullanilan application servisleri `Services` klasorunde tutulmalidir.
- `Services` altinda iki rol net ayrilmalidir:
  - `DomainService`: is kuralini uygular
  - `AppService`: use-case akislarini ve orkestrasyonu yonetir
- Handler'lar domain davranisini aggregate uzerinden calistirir; teknik detaylara inmez.
- Cross-cutting davranislar MediatR pipeline benzeri `Behaviors` klasorunde tutulabilir.
- Validation burada yapilir; persistence tarafina gecmeden request dogrulanir.

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

### 3) Infrastructure

- `DbContext`, EF configuration, repository implementasyonlari, `UnitOfWork`, seed ve migration burada olur.
- JWT token ureteci, password/secret hasher, cache adapter, event publisher gibi teknik servisler burada yasar.
- OKS kullaniliyorsa bu katman `Oks.Persistence.EfCore`, `Oks.Logging`, `Oks.Caching` gibi modullerle compose edilir.
- Ayrica `AuthrDbContext` bu bounded context icin ayri connection string ile calismalidir.
- Ayni mantik burada da gecerlidir: tek tek sinif isimleri yerine `Context`, `Configurations`, `Repositories`, `Integrations`, `DependencyInjection` gibi sorumluluk bazli klasorleme tercih edilmelidir.

### 4) API

- Controller ve middleware burada olur.
- API katmani sadece request'i `Application` use-case'lerine yonlendirir.
- Exception handling, correlation, auth middleware, rate limit, validation ve result wrapping bu katmanda compose edilir.
- `Program.cs` icinde yalnizca composition olmalidir; is kurali olmamalidir.
- API tarafinda da `Controllers`, `Middleware`, `Filters`, `OpenApi`, `HealthChecks`, `Configuration` gibi delivery odakli klasorleme tercih edilmelidir.

## AggregateRoot bazli klasorleme notu

Sizin istediginiz gibi `Application` icinde her AggregateRoot altinda `Services`, `Commands` ve `Queries` tutmak iyi bir secimdir. Bu yapida her use-case klasoru kendi `RequestDto`, `ResponseDto`, `Handler` ve `Validator` dosyalariyla birlikte durur. Entity icindeki davranislar da ayni aggregate altindaki `Services` klasorune tasinir. Bu, ozellikle buyuyen auth alanlarinda teknik degil is odakli gezinme saglar.

Ancak su dengeyi koruyun:

- AggregateRoot olmayan ama merkezi use-case olan `Auth` klasoru ayri kalabilir.
- `Login`, `RefreshToken`, `Logout` gibi akislari sadece `Users` altina sikistirmayin.
- `Sessions` ve `Auth` bazen ayri uygulama modulu gibi ele alinabilir.

## OKS ile uyumlu composition onerisi

`Authr.Infrastructure` ve `Authr.API` tarafinda su entegrasyonlar mantiklidir:

- `AddOksEfCore<AuthrDbContext>()`
- `AddOksCurrentUserProvider()`
- `AddOksUnitOfWork()`
- `AddOksResultWrapping()`
- `AddOksValidation()`
- `AddOksRepositoryLogging()`
- gerekiyorsa `AddOksCaching()` ve permission/session query'lerinde selective cache

Onerilen sinir:

- `Domain`: OKS'e dogrudan baglanmasin.
- `Application`: mumkunse sadece abstraction bilsin.
- `Infrastructure` ve `API`: OKS modullerini burada compose etsin.

## Auth/Authr icin deployment secenekleri

Bu bounded context iki sekilde kullanilabilir:

- ayni container icinde modul olarak
- ayri container ve ayri deployment olarak

Ayri container ve ayri DB planliyorsaniz su yapilar eklenmeli:

- Ayri `AuthrDbContext`
- Ayri migration history
- Ayri health check
- Ayri connection string
- Ayri secret management
- Ayri log category veya sink ayrimi
- Ayri cache namespace/prefix
- Ayri deployment pipeline

Bu ayri deployment modeli su avantajlari verir:

- Authentication servisi bagimsiz olceklenir.
- Guvenlik kurallari ve secret rotasyonu ayrisir.
- Login/refresh trafigi, is domain'i DB'sini etkilemez.
- Ileride token/introspection ya da OpenIddict sunucusu ayri buyutulebilir.

Ayni container icinde kullanilacaksa da su sinir korunmalidir:

- ayri `AuthrDbContext`
- ayri migration seti
- ayri configuration section
- ayri application klasorleme ve use-case siniri
- auth concern'lerinin diger is modullerine sizmamasi

## Bu yapiya eklenmesi gereken hususlar

Evet, bence su basliklar da eklenmeli:

- `Domain/Specifications/`
Auth tarafinda aktif kullanici, gecerli client, refresh token durumu gibi filtreler icin faydali olabilir.

- `Domain/Policies/`
Password policy, lockout policy, client policy gibi kurallar burada netlesebilir.

- `Application/Common/`
Paged response, mapping, shared exceptions, authorization requirement modelleri icin ortak alan faydali olur.

- `Infrastructure/Outbox/`
Auth olaylarini diger servislere yayinlayacaksaniz outbox dusunulmeli.

- `API/OpenApi/`
Token endpoint'leri, admin endpoint'leri ve internal endpoint'ler icin ayrik Swagger gruplama ise yarar.

- `tests/Integration/Fixtures/`
Ayri container/db kullandiginiz icin test fixture ve testcontainer tabanli hazirlik katmani degerlidir.

## Kisa isimlendirme kurallari

- Solution ve proje adlarinda tek stil secin: `Authr.*` veya `Auth.*`
- AggregateRoot klasorleri cogul olabilir: `Users`, `Roles`, `Clients`
- Command/Query isimleri fiil ile baslasin: `CreateClientCommand`, `GetUserByIdQuery`
- DTO isimleri amac belirtmeli: `LoginRequestDto`, `LoginResponseDto`
- `Services` klasorunde yalnizca application/domain koordinasyonu olsun; teknik adapter'lar `Infrastructure`'da kalsin

## Onerilen minimum baslangic seti

Eger ilk surumu sade baslatmak istiyorsaniz su cekirdek yeterli:

1. `Authr.Domain`
2. `Authr.Application`
3. `Authr.Infrastructure`
4. `Authr.API`
5. `AuthrDbContext`
6. `User`, `Role`, `Client`, `RefreshToken` aggregate/entitiy seti
7. `Login`, `RefreshToken`, `Logout`, `CreateClient`, `GetUserById` use-case'leri
8. Validation, unit of work, repository logging

Bu iskelet, sonradan OpenIddict, event bus, outbox, distributed cache ve multi-tenant genislemelerine uygun kalir.
