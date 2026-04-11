# Write Repository & Unit of Work - Description

[Ana sayfa](../README.md)

Yazma operasyonlari icin OKS, `IWriteRepository<TEntity, TKey>` ve `EfUnitOfWork` kombinasyonunu sunar. `AddOksUnitOfWork()` ile MVC action'larda filter uzerinden otomatik `SaveChangesAsync` denenir; Minimal API tarafinda ayni davranis `IEndpointFilter` tabanli `AddOksUnitOfWork()` (RouteGroupBuilder extension) ile saglanir. `[OksSkipTransaction]` ile davranis endpoint/action bazinda kapatilabilir.

> Onemli: `IWriteRepository<TEntity, TKey>`, `IReadRepository<TEntity, TKey>` arayuzunden turer. Bu nedenle write repository alan bir servis, ayni nesne uzerinden read metodlarini da kullanabilir. Ayni handler veya service icinde hem `IReadRepository` hem `IWriteRepository` enjekte etmek cogu durumda gerekmez.

## Baslica bilesenler
> Mimari not (2026-03-30): `Oks.Web` write pipeline tarafinda yalnizca abstraction katmanlarina bagimlidir; `IUnitOfWork`/repository concrete kayitlari host tarafinda (`Oks.Persistence.EfCore` veya alternatif implementasyon) compose edilir.

- **Oks.Persistence.Abstractions**: `IWriteRepository`, unit of work ve transaction kontratlari.
- **Oks.Persistence.EfCore**: EF Core implementasyonlari, audit ve soft delete destegi.
- **Oks.Web**: MVC filter + Minimal API endpoint filter ile otomatik commit davranisi.
- **Oks.Domain**: `Entity`, `AuditedEntity` ve `IAuditedEntity` taban tipleri ile audit alanlarini standartlastirir.

## Neler saglar?
- Action/endpoint sonunda otomatik commit, yazma olmadiginda bos commit yapilmaz.
- MVC action'larda `OksUnitOfWorkFilter`, Minimal API'lerde `OksMinimalApiUnitOfWorkFilter` otomatik commit dener.
- `[OksSkipTransaction]` ile davranis tamamen devre disi birakilabilir.
- Audit alanlari (`CreatedAt`, `CreatedBy`, `ModifiedAt`, `ModifiedBy`) ve soft delete otomatik doldurulur.
- Okuma + yazma yapan siniflar tek bagimlilik olarak `IWriteRepository` kullanabilir; sadece okuma yapan siniflarda `IReadRepository` tercih edilmelidir.

---
## Usage

Kurulum ve kopyala-yapistir kod ornekleri icin: [WriteRepository_Usage.md](WriteRepository_Usage.md)
