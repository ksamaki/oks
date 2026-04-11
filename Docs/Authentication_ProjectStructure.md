# Authentication - Project Structure

[Ana sayfa](../README.md) | [Authentication - Description](Authentication_Description.md) | [Authentication - Usage](Authentication_Usage.md) | [Authentication - Architecture](Authentication_Architecture.md)

Bu dokuman, yeni bir projede `Auth` / `Authr` bounded context'ini CQRS, DDD ve AggregateRoot odakli bir klasorleme ile baslatmak icin ornek bir yapi sunar. Hedef; OKS'nin repository, unit of work, validation, logging, caching ve web pipeline yetenekleriyle uyumlu, buyudukce bozulmayan bir iskelet vermektir.

> Mimari not: `Auth` / `Authr` icin ayri container ve ayri database planlamak dogru bir tercih. Authentication, diger bounded context'lerden operasyonel olarak ayrismaya en uygun alanlardan biridir. OKS tarafinda da bu ayrim, ayri `DbContext`, ayri migration seti, ayri deployment ve gerekirse ayri cache/policy konfigrasyonu ile rahatca desteklenir.

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
        User.cs
        UserSession.cs
        UserClaim.cs
        UserDomainEvents.cs
      Roles/
        Role.cs
        RolePermission.cs
      Clients/
        Client.cs
      RefreshTokens/
        RefreshToken.cs
    Common/
      AggregateRoot.cs
      Entity.cs
      ValueObject.cs
      IHasDomainEvents.cs
    Enums/
      ClientType.cs
      GrantType.cs
      PermissionScope.cs
      UserStatus.cs
    ValueObjects/
      EmailAddress.cs
      PasswordHash.cs
      ClientSecretHash.cs
      IpAddress.cs
      DeviceFingerprint.cs
    Events/
      UserLoggedInDomainEvent.cs
      RefreshTokenRotatedDomainEvent.cs
      UserLockedOutDomainEvent.cs
    Services/
      IPasswordPolicy.cs
      IClientPolicy.cs
    Repositories/
      IUserRepository.cs
      IRoleRepository.cs
      IClientRepository.cs
      IRefreshTokenRepository.cs

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
        Commands/
          RegisterUser/
            RegisterUserCommand.cs
            RegisterUserCommandHandler.cs
            RegisterUserCommandValidator.cs
            RegisterUserRequestDto.cs
            RegisterUserResponseDto.cs
          ChangePassword/
            ChangePasswordCommand.cs
            ChangePasswordCommandHandler.cs
            ChangePasswordCommandValidator.cs
          LockUser/
            LockUserCommand.cs
            LockUserCommandHandler.cs
        Queries/
          GetUserById/
            GetUserByIdQuery.cs
            GetUserByIdQueryHandler.cs
            GetUserByIdResponseDto.cs
          GetUserPermissions/
            GetUserPermissionsQuery.cs
            GetUserPermissionsQueryHandler.cs
            GetUserPermissionsResponseDto.cs
        Dtos/
          UserDto.cs
          UserClaimDto.cs
          UserSessionDto.cs
        Interfaces/
          IUserReadService.cs
        Services/
          UserPermissionResolver.cs
      Roles/
        Commands/
          CreateRole/
            CreateRoleCommand.cs
            CreateRoleCommandHandler.cs
            CreateRoleCommandValidator.cs
        Queries/
          GetRoleById/
            GetRoleByIdQuery.cs
            GetRoleByIdQueryHandler.cs
            GetRoleByIdResponseDto.cs
        Dtos/
          RoleDto.cs
        Interfaces/
          IRoleReadService.cs
        Services/
          RolePermissionMapper.cs
      Clients/
        Commands/
          CreateClient/
            CreateClientCommand.cs
            CreateClientCommandHandler.cs
            CreateClientCommandValidator.cs
        Queries/
          GetClientById/
            GetClientByIdQuery.cs
            GetClientByIdQueryHandler.cs
            GetClientByIdResponseDto.cs
        Dtos/
          ClientDto.cs
        Interfaces/
          IClientReadService.cs
        Services/
          ClientSecretService.cs
      Sessions/
        Commands/
          RevokeSession/
            RevokeSessionCommand.cs
            RevokeSessionCommandHandler.cs
            RevokeSessionCommandValidator.cs
        Queries/
          GetActiveSessions/
            GetActiveSessionsQuery.cs
            GetActiveSessionsQueryHandler.cs
            GetActiveSessionsResponseDto.cs
      Auth/
        Commands/
          Login/
            LoginCommand.cs
            LoginCommandHandler.cs
            LoginCommandValidator.cs
            LoginRequestDto.cs
            LoginResponseDto.cs
          RefreshToken/
            RefreshTokenCommand.cs
            RefreshTokenCommandHandler.cs
            RefreshTokenCommandValidator.cs
            RefreshTokenResponseDto.cs
          Logout/
            LogoutCommand.cs
            LogoutCommandHandler.cs
        Queries/
          IntrospectToken/
            IntrospectTokenQuery.cs
            IntrospectTokenQueryHandler.cs
            IntrospectTokenResponseDto.cs
        Dtos/
          AccessTokenDto.cs
          RefreshTokenDto.cs
        Interfaces/
          IAuthenticationService.cs
        Services/
          AuthenticationOrchestrator.cs

  Authr.Infrastructure/
    Authr.Infrastructure.csproj
    Persistence/
      AuthrDbContext.cs
      Configurations/
        UserConfiguration.cs
        RoleConfiguration.cs
        ClientConfiguration.cs
        RefreshTokenConfiguration.cs
      Repositories/
        UserRepository.cs
        RoleRepository.cs
        ClientRepository.cs
        RefreshTokenRepository.cs
      Migrations/
      Seed/
        AuthrSeeder.cs
      UnitOfWork/
        AuthUnitOfWork.cs
    Identity/
      PasswordHasher.cs
      SecretHasher.cs
      JwtTokenGenerator.cs
      PermissionClaimsFactory.cs
    Integrations/
      Caching/
        UserPermissionCache.cs
      Logging/
        AuthAuditLogger.cs
      Messaging/
        DomainEventPublisher.cs
      Time/
        SystemDateTimeProvider.cs
    DependencyInjection/
      ServiceCollectionExtensions.cs

  Authr.API/
    Authr.API.csproj
    Controllers/
      AuthController.cs
      UsersController.cs
      RolesController.cs
      ClientsController.cs
    Middleware/
      CorrelationMiddleware.cs
      ExceptionHandlingMiddleware.cs
      TenantResolutionMiddleware.cs
    Filters/
      ApiExceptionFilter.cs
    Extensions/
      ServiceCollectionExtensions.cs
      ApplicationBuilderExtensions.cs
    Contracts/
      Requests/
      Responses/
    Program.cs

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
Ornek: `IUserRepository`, `IClientRepository`
- Enum, value object ve domain event'ler dogrudan aggregate davranisini desteklemelidir.
- Login gibi saf orkestrasyon use-case'leri burada degil `Application` katmaninda olmalidir.

### 2) Application

- CQRS handler'lari burada olur.
- Her AggregateRoot icin ayri klasor acmak, use-case'leri is alanina gore toplar.
- `Commands`, `Queries`, `Dtos`, `Validators`, `Interfaces`, `Services` alt klasorleri, sizin istediginiz standart olarak uygundur.
- Handler'lar domain davranisini aggregate uzerinden calistirir; teknik detaylara inmez.
- Cross-cutting davranislar MediatR pipeline benzeri `Behaviors` klasorunde tutulabilir.
- Validation burada yapilir; persistence tarafina gecmeden request dogrulanir.

### 3) Infrastructure

- `DbContext`, EF configuration, repository implementasyonlari, `UnitOfWork`, seed ve migration burada olur.
- JWT token ureteci, password/secret hasher, cache adapter, event publisher gibi teknik servisler burada yasar.
- OKS kullaniliyorsa bu katman `Oks.Persistence.EfCore`, `Oks.Logging`, `Oks.Caching` gibi modullerle compose edilir.
- Ayrica `AuthrDbContext` bu bounded context icin ayri connection string ile calismalidir.

### 4) API

- Controller ve middleware burada olur.
- API katmani sadece request'i `Application` use-case'lerine yonlendirir.
- Exception handling, correlation, auth middleware, rate limit, validation ve result wrapping bu katmanda compose edilir.
- `Program.cs` icinde yalnizca composition olmalidir; is kurali olmamalidir.

## AggregateRoot bazli klasorleme notu

Sizin istediginiz gibi `Application` icinde her AggregateRoot altinda `Commands`, `Queries`, `Dtos`, `Validators`, `Interfaces`, `Services` tutmak iyi bir secimdir. Bu, ozellikle buyuyen auth alanlarinda teknik degil is odakli gezinme saglar.

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

## Auth/Authr icin ayri container ve DB notlari

Bu bounded context icin ayri container ve ayri DB planliyorsaniz su yapilar eklenmeli:

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
