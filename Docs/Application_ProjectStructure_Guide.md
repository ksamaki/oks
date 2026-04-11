# Application - Project Structure Guide

[Ana sayfa](../README.md)

Bu dokuman, OKS kullanan ekiplerin yeni bir uygulamaya baslarken hangi library ve klasor yapisini kurmasi gerektigini netlestirmek icin hazirlanmistir. Amaç; her projede yeniden ayni tartismalari yapmadan, tutarli ve buyuyebilir bir baslangic iskeleti vermektir.

Bu rehber:

- genel uygulama baslangic yapisini tarif eder
- CQRS + DDD + Clean Architecture uyumlu dusunur
- OKS'nin validation, repository, unit of work, logging, cache ve web katmani yeteneklerini dikkate alir
- hem monolith hem de bounded-context bazli ayristirilmis yapiya uyarlanabilir

## Ne zaman bu yapiyi kullanmali?

Su tip projelerde bu yapi dogrudan uygundur:

- orta ve buyuk olcekli line-of-business uygulamalari
- birden fazla module / bounded context iceren sistemler
- API-first backend projeleri
- CQRS veya use-case odakli servisler
- ileride moduler monolith veya mikroservise evrilebilecek uygulamalar

Kucuk CRUD uygulamalarda bunun daha sade bir varyanti tercih edilebilir; ama yine de katman sinirlari korunmalidir.

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

Bu katman is kurallarinin kalbidir.

Icerik:

- entity
- aggregate root
- enum
- value object
- domain event
- domain service
- repository kontratlari
- policy / specification

Onerilen yapi:

```text
MyApp.Domain/
  Aggregates/
    Orders/
      Order.cs
      OrderItem.cs
    Customers/
      Customer.cs
  Common/
    Entity.cs
    AggregateRoot.cs
    ValueObject.cs
  Enums/
  Events/
  Policies/
  Specifications/
  Repositories/
    IOrderRepository.cs
    ICustomerRepository.cs
```

Kurallar:

- `Domain`, `Infrastructure` bilmez
- teknik servisleri bilmez
- framework bagimliligini minimumda tutar
- aggregate root kendi invariant'larini korur

### 2) `MyApp.Application`

Bu katman use-case katmanidir. CQRS handler'lari burada yer alir.

Icerik:

- commands
- queries
- handlers
- DTO
- validators
- application service
- interface / port
- behavior / pipeline

Onerilen yapi:

```text
MyApp.Application/
  Common/
    Behaviors/
      ValidationBehavior.cs
      LoggingBehavior.cs
      TransactionBehavior.cs
    Exceptions/
    Models/
  Aggregates/
    Orders/
      Commands/
        CreateOrder/
          CreateOrderCommand.cs
          CreateOrderCommandHandler.cs
          CreateOrderCommandValidator.cs
      Queries/
        GetOrderById/
          GetOrderByIdQuery.cs
          GetOrderByIdQueryHandler.cs
      Dtos/
        OrderDto.cs
      Interfaces/
      Services/
    Customers/
      Commands/
      Queries/
      Dtos/
      Interfaces/
      Services/
  Abstractions/
    Persistence/
    Time/
    Messaging/
    Security/
```

Kurallar:

- application katmani teknik detay implement etmez
- handler'lar is akisini koordine eder
- validation burada yapilir
- domain davranisi aggregate uzerinden calistirilir

### 3) `MyApp.Infrastructure`

Bu katman teknik implementasyon katmanidir.

Icerik:

- EF Core DbContext
- repository implementasyonlari
- unit of work
- migration
- seed
- cache adapter
- log adapter
- external servis adapter'lari
- background job implementasyonlari

Onerilen yapi:

```text
MyApp.Infrastructure/
  Persistence/
    MyAppDbContext.cs
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
  DependencyInjection/
    ServiceCollectionExtensions.cs
```

Kurallar:

- `Domain` ve `Application` kontratlarini implement eder
- OKS modulleri burada compose edilebilir
- connection string, provider ve repository secimleri burada kalir

### 4) `MyApp.API`

Bu katman uygulamanin disa acilan katmanidir.

Icerik:

- controller
- middleware
- filter
- endpoint mapping
- OpenAPI / Swagger
- appsettings
- startup composition

Onerilen yapi:

```text
MyApp.API/
  Controllers/
  Middleware/
  Filters/
  Extensions/
  OpenApi/
  HealthChecks/
  Program.cs
  appsettings.json
  appsettings.Development.json
```

Kurallar:

- is kurali burada yazilmaz
- yalnizca request alir, use-case'e yonlendirir
- validation, exception handling, auth, rate limit, result wrapping burada compose edilir

### 5) `MyApp.Contracts`

Bu katman opsiyoneldir ama faydalidir.

Ne zaman gerekir:

- baska servislerle request/response modeli paylasilacaksa
- internal SDK veya client package uretilecekse
- HTTP kontratlari application DTO'larindan ayri tutulmak isteniyorsa

Onerilen yapi:

```text
MyApp.Contracts/
  Requests/
  Responses/
  Events/
```

## Klasorleme mantigi: teknik mi is alani mi?

OKS kullanan yeni projelerde varsayilan tercih su olmali:

- `Domain` icinde aggregate / is alani bazli klasorleme
- `Application` icinde aggregate bazli klasorleme
- `Infrastructure` icinde teknik klasorleme
- `API` icinde delivery mekanizmasi bazli klasorleme

Yani:

- `Orders`, `Customers`, `Invoices` gibi is alanlari `Domain` ve `Application` icinde oncelikli olur
- `Persistence`, `Caching`, `Logging`, `Messaging` gibi teknik alanlar `Infrastructure` icinde oncelikli olur

Bu denge en okunabilir yapidir.

## OKS ile minimum entegrasyon noktasi

Yeni bir uygulama baslangicinda en sik gereken OKS entegrasyonlari:

- `AddOksEfCore<MyAppDbContext>()`
- `AddOksCurrentUserProvider()`
- `AddOksUnitOfWork()`
- `AddOksResultWrapping()`
- `AddOksValidation()`
- `AddOksRepositoryLogging()`

Ihtiyaca gore:

- `AddOksCaching()`
- `AddOksRateLimiting()`
- `AddOksPerformanceLogging()`
- `AddOksAuthentication()`

Onerilen katman siniri:

- `Domain`: OKS bilmez
- `Application`: mumkunse abstraction disinda bir sey bilmez
- `Infrastructure`: repository, cache, logging tarafinda OKS compose eder
- `API`: web pipeline tarafinda OKS compose eder

## Yeni proje acarken minimum baslangic checklist'i

1. `Domain`, `Application`, `Infrastructure`, `API` projelerini ac
2. gerekiyorsa `Contracts` projesini ekle
3. `DbContext` ve migration stratejisini belirle
4. aggregate root listesini cikar
5. ilk use-case listesini cikar
6. validation, unit of work ve exception pipeline'i kur
7. repository ve logging entegrasyonunu ekle
8. health check ve integration test iskeletini ekle
9. `docs/architecture.md` ve `docs/conventions.md` dosyalarini ac

## Bence mutlaka eklenmesi gereken ek alanlar

- `docs/conventions.md`
Naming, katman kurallari, commit/branch ve code style kararlarini toplamak icin.

- `tests/IntegrationTests/Fixtures/`
Gercek DB ve container tabanli testler icin.

- `API/HealthChecks/`
DB, cache, external dependency health kontrolleri icin.

- `Infrastructure/Observability/`
Log, metric ve tracing adapter'larini toplamak icin.

- `Application/Common/`
Ortak exception, response modeli ve behavior'lar icin.

## Hangi durumda ayri servis / ayri bounded context dusunulmeli?

Su moduller ayri servis veya en azindan ayri bounded context olmaya adaydir:

- Authentication / Authorization
- Notification
- File / Media
- Payment
- Realtime / Gateway

Su moduller ise genelde ana uygulama icinde baslayabilir:

- katalog
- musteri
- siparis
- raporlama

## Onerilen ilk isimlendirme standardi

- solution: `MyApp.slnx`
- domain: `MyApp.Domain`
- application: `MyApp.Application`
- infrastructure: `MyApp.Infrastructure`
- api: `MyApp.API`
- contracts: `MyApp.Contracts`

Aggregate klasorleri:

- `Orders`
- `Customers`
- `Invoices`

Use-case isimleri:

- `CreateOrderCommand`
- `CancelOrderCommand`
- `GetOrderByIdQuery`
- `ListCustomerOrdersQuery`

## Kisa karar ozeti

Varsayilan baslangic yapisi olarak sunu oneriyorum:

1. `Domain`
2. `Application`
3. `Infrastructure`
4. `API`
5. opsiyonel `Contracts`
6. aggregate bazli `Application` klasorleme
7. teknik bazli `Infrastructure` klasorleme
8. OKS entegrasyonunu yalnizca `Infrastructure` ve `API` katmaninda yapmak

Bu yapi, yeni baslayan ekipler icin yeterince net; buyuyen projeler icin de yeterince esnektir.
