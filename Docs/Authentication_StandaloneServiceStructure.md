# Authentication - Standalone Service Structure

[Ana sayfa](../README.md) | [Authentication - Description](Authentication_Description.md) | [Authentication - Usage](Authentication_Usage.md) | [Authentication - Project Structure](Authentication_ProjectStructure.md)

Bu dokuman, Authentication alanini ayri deploy edilen servis olarak kurmak isteyen projeler icin hazirlanmistir. Ornek isimlendirme `Authr` uzerindendir.

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
    k8s/
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
    Users/
      Services/
        UserAppService.cs
        IUserAppService.cs
        UserDomainService.cs
        IUserDomainService.cs
      Commands/
        RegisterUser/
          RegisterUserCommand.cs
          RegisterUserCommandHandler.cs
          RegisterUserCommandValidator.cs
      Queries/
        GetUserById/
          GetUserByIdQuery.cs
          GetUserByIdQueryHandler.cs
          GetUserByIdDto.cs
          GetUserByIdQueryValidator.cs
    Roles/
      Services/
        RoleAppService.cs
        IRoleAppService.cs
        RoleDomainService.cs
        IRoleDomainService.cs
      Commands/
        CreateRole/
          CreateRoleCommand.cs
          CreateRoleCommandHandler.cs
          CreateRoleCommandValidator.cs
      Queries/
        GetRoleById/
          GetRoleByIdQuery.cs
          GetRoleByIdQueryHandler.cs
          GetRoleByIdDto.cs
          GetRoleByIdQueryValidator.cs
    Clients/
      Services/
        ClientAppService.cs
        IClientAppService.cs
        ClientDomainService.cs
        IClientDomainService.cs
      Commands/
        CreateClient/
          CreateClientCommand.cs
          CreateClientCommandHandler.cs
          CreateClientCommandValidator.cs
      Queries/
        GetClientById/
          GetClientByIdQuery.cs
          GetClientByIdQueryHandler.cs
          GetClientByIdDto.cs
          GetClientByIdQueryValidator.cs
    Auth/
      Services/
        AuthAppService.cs
        IAuthAppService.cs
        AuthDomainService.cs
        IAuthDomainService.cs
      Commands/
        Login/
          LoginCommand.cs
          LoginCommandHandler.cs
          LoginCommandValidator.cs
        RefreshToken/
          RefreshTokenCommand.cs
          RefreshTokenCommandHandler.cs
          RefreshTokenCommandValidator.cs
        Logout/
          LogoutCommand.cs
          LogoutCommandHandler.cs
          LogoutCommandValidator.cs
      Queries/
        IntrospectToken/
          IntrospectTokenQuery.cs
          IntrospectTokenQueryHandler.cs
          IntrospectTokenDto.cs
          IntrospectTokenQueryValidator.cs

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

## Application klasorleme standardi

- `Command` kendi request modelidir
- `Query` kendi request modelidir
- command tarafinda ekstra DTO uretilmez
- query tarafinda gerekiyorsa tek `Dto` response modeli kullanilir
- `Services/` altinda:
  - `AppService`: use-case akislarini ve orkestrasyonu yonetir
  - `DomainService`: is kuralini uygular

Ornek:

```text
Aggregates/
  Users/
    Services/
      UserAppService.cs
      IUserAppService.cs
      UserDomainService.cs
      IUserDomainService.cs
    Commands/
      RegisterUser/
        RegisterUserCommand.cs
        RegisterUserCommandHandler.cs
        RegisterUserCommandValidator.cs
    Queries/
      GetUserById/
        GetUserByIdQuery.cs
        GetUserByIdQueryHandler.cs
        GetUserByIdDto.cs
        GetUserByIdQueryValidator.cs
```

## Domain model notu

- entity icinde constructor bulunmaz
- entity icinde davranis methodu bulunmaz
- veri tasiyan domain tipleri `Domain` katmaninda kalir
- is kurali `DomainService` ile uygulanir
- use-case orkestrasyonu `AppService` ile yonetilir

## Deployment notu

Bu servis ayri container olarak calisacak sekilde tasarlanir. Ancak ayni klasorleme mantigi, daha sonra monolith icine modul olarak alinacak yapilarda da korunabilir.
