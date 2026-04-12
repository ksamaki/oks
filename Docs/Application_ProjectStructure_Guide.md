# Application - Project Structure Guide

[Ana sayfa](../README.md)

Bu dokuman, OKS kullanan ekiplerin yeni bir uygulamaya baslarken hangi library ve klasor yapisini kurmasi gerektigini netlestirmek icin hazirlanmistir. Amac; her projede yeniden ayni tartismalari yapmadan, tutarli ve buyuyebilir bir baslangic iskeleti vermektir.

Bu rehber:

- genel uygulama baslangic yapisini tarif eder
- CQRS + DDD + Clean Architecture uyumlu dusunur
- OKS'nin validation, repository, unit of work, logging, cache ve web katmani yeteneklerini dikkate alir
- hem monolith hem de bounded-context bazli ayristirilmis yapiya uyarlanabilir

## Onerilen ust seviye yapi

```text
MyApp/
  README.md
  docker-compose.yml
  .env
  docs/
    architecture.md
    deployment.md
    conventions.md
  src/
    MyApp.slnx
    MyApp.Domain/
    MyApp.Application/
    MyApp.Infrastructure/
    MyApp.API/
    MyApp.Contracts/
  tests/
    MyApp.Domain.Tests/
    MyApp.Application.Tests/
    MyApp.Infrastructure.Tests/
    MyApp.API.Tests/
    MyApp.IntegrationTests/
```

## Onerilen library yapisi

### 1) `MyApp.Domain`

```text
MyApp.Domain/
  Aggregates/
    Orders/
      Entities/
      Events/
      ValueObjects/
    Customers/
      Entities/
      Events/
      ValueObjects/
  Common/
  Enums/
  Events/
  Policies/
  Specifications/
  Repositories/
```

Kurallar:

- `Domain`, `Infrastructure` bilmez
- entity icinde constructor ve davranis methodu bulunmaz
- entity davranislari `Application` katmanindaki servisler tarafindan yonetilir
- OksFramework'te zaten var olan ortak altyapi tipleri burada tekrar edilmez

### 2) `MyApp.Application`

```text
MyApp.Application/
  Common/
    Behaviors/
    Exceptions/
    Models/
  Aggregates/
    Orders/
      Services/
        OrderAppService.cs
        IOrderAppService.cs
        OrderDomainService.cs
        IOrderDomainService.cs
      Commands/
        CreateOrder/
          CreateOrderCommand.cs
          CreateOrderCommandHandler.cs
          CreateOrderCommandValidator.cs
      Queries/
        GetOrderById/
          GetOrderByIdQuery.cs
          GetOrderByIdQueryHandler.cs
          GetOrderByIdDto.cs
          GetOrderByIdQueryValidator.cs
    Customers/
      Services/
        CustomerAppService.cs
        ICustomerAppService.cs
        CustomerDomainService.cs
        ICustomerDomainService.cs
      Commands/
      Queries/
  Abstractions/
    Persistence/
    Time/
    Messaging/
    Security/
```

Kurallar:

- command kendi request modelidir
- query kendi request modelidir
- command tarafinda ekstra DTO uretilmez
- query tarafinda gerekiyorsa tek `Dto` response modeli tutulur
- `Services/` altinda:
  - `AppService`: akisi ve orkestrasyonu yonetir
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

### 3) `MyApp.Infrastructure`

```text
MyApp.Infrastructure/
  Persistence/
    Context/
    Configurations/
    Repositories/
    UnitOfWork/
    Migrations/
    Seed/
  Integrations/
    Cache/
    Logging/
    Messaging/
    Time/
    Security/
  DependencyInjection/
```

Kurallar:

- `Domain` ve `Application` kontratlarini implement eder
- OKS modulleri burada compose edilir
- sorumluluk bazli klasorleme tercih edilir

### 4) `MyApp.API`

```text
MyApp.API/
  Controllers/
  Middleware/
  Filters/
  Extensions/
  OpenApi/
  HealthChecks/
  Configuration/
```

Kurallar:

- is kurali burada yazilmaz
- sadece request alir ve use-case'e yonlendirir
- validation, exception handling, auth, rate limit, result wrapping burada compose edilir

### 5) `MyApp.Contracts`

```text
MyApp.Contracts/
  Requests/
  Responses/
  Events/
```

## Kisa karar ozeti

Varsayilan baslangic yapisi olarak sunu oneriyorum:

1. `Domain`
2. `Application`
3. `Infrastructure`
4. `API`
5. opsiyonel `Contracts`
6. `Application` icinde aggregate bazli klasorleme
7. `Command` ve `Query` siniflarini request modeli olarak kullanmak
8. command tarafinda ekstra DTO uretmemek
9. query tarafinda gerekiyorsa tek `Dto` response modeli kullanmak
10. aggregate seviyesinde `AppService` ve `DomainService` ayrimi yapmak
11. OKS entegrasyonunu yalnizca `Infrastructure` ve `API` katmaninda yapmak

Bu yapi, yeni baslayan ekipler icin yeterince net; buyuyen projeler icin de yeterince esnektir.
