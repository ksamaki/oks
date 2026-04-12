# Authentication - Project Structure

[Ana sayfa](../README.md) | [Authentication - Description](Authentication_Description.md) | [Authentication - Usage](Authentication_Usage.md) | [Authentication - Architecture](Authentication_Architecture.md)

Bu dokuman, yeni bir projede `Auth` / `Authr` bounded context'ini CQRS, DDD ve AggregateRoot odakli bir klasorleme ile baslatmak icin ornek bir yapi sunar.

> Mimari not: `Auth` / `Authr`, ihtiyaca gore ayni container icinde modul olarak da ayri container/ayri deployment olarak da kullanilabilir.

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
      Security/
      Persistence/
      Messaging/
    Behaviors/
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
          ChangePassword/
            ChangePasswordCommand.cs
            ChangePasswordCommandHandler.cs
            ChangePasswordCommandValidator.cs
          LockUser/
            LockUserCommand.cs
            LockUserCommandHandler.cs
            LockUserCommandValidator.cs
        Queries/
          GetUserById/
            GetUserByIdQuery.cs
            GetUserByIdQueryHandler.cs
            GetUserByIdDto.cs
            GetUserByIdQueryValidator.cs
          GetUserPermissions/
            GetUserPermissionsQuery.cs
            GetUserPermissionsQueryHandler.cs
            GetUserPermissionsDto.cs
            GetUserPermissionsQueryValidator.cs
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
      Sessions/
        Services/
          SessionAppService.cs
          ISessionAppService.cs
          SessionDomainService.cs
          ISessionDomainService.cs
        Commands/
          RevokeSession/
            RevokeSessionCommand.cs
            RevokeSessionCommandHandler.cs
            RevokeSessionCommandValidator.cs
        Queries/
          GetActiveSessions/
            GetActiveSessionsQuery.cs
            GetActiveSessionsQueryHandler.cs
            GetActiveSessionsDto.cs
            GetActiveSessionsQueryValidator.cs
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
```

## Application standardi

- `Command` kendi request modelidir
- `Query` kendi request modelidir
- command tarafinda ekstra DTO kullanilmaz
- query tarafinda gerekiyorsa tek `Dto` response modeli kullanilir
- `Services/` altinda:
  - `AppService`: akisi ve orkestrasyonu yonetir
  - `DomainService`: is kuralini uygular

## Domain standardi

- entity icinde constructor bulunmaz
- entity icinde davranis methodu bulunmaz
- davranislar `Application/Aggregates/.../Services` altina tasinir

## Deployment secenekleri

- ayni container icinde modul olarak
- ayri container ve ayri deployment olarak

Iki durumda da `Authr` klasorleme ve use-case siniri korunmalidir.
