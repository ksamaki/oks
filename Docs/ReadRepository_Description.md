# Read-only Repository - Description

[Ana sayfa](../README.md)

OKS read-only repository katmani, veri okumayi yazma islerinden ayirarak CQRS dostu ve test edilebilir bir altyapi saglar. Bu yapida temel okuma standardi `IQueryable` merkezlidir.

`IReadRepository<TEntity, TKey>` su yetenekleri saglar:

- `GetByIdAsync`
- `GetListAsync`
- `Query()`
- `Query(predicate)`

Tek kayit / cok kayit / var-yok / sayi gibi semantik ayrimlar repository isimleriyle degil, LINQ async operatorleriyle yapilir:

- `FirstOrDefaultAsync`
- `SingleOrDefaultAsync`
- `AnyAsync`
- `CountAsync`
- `ToListAsync`

Bu yaklasim, buyuk frameworklerdeki `IQueryable` merkezli sorgulama mantigina daha yakindir.

## Baslica bilesenler
> Mimari not (2026-04-12): `Oks.Web` katmani abstraction-only bagimlilik kurali ile calisir; concrete persistence/logging/caching implementasyonlari host tarafinda compose edilir.

- **Oks.Persistence.Abstractions**: `IReadRepository` arayuzu ve ortak modeller.
- **Oks.Persistence.EfCore**: EF Core implementasyonu, `AsNoTracking` varsayilanlari ve soft-delete query filter.
- **Oks.Web**: HTTP tabanli current user provider entegrasyonu (`IOksUserProvider`).

## Neler saglar?

- `GetByIdAsync` ile PK bazli tek kayit erisimi
- `GetListAsync(predicate)` ile filtreli liste cekme
- `GetListAsync()` ile tum listeyi cekebilme
- `Query()` ve `Query(predicate)` ile sorting, paging ve ileri query kompozisyonu
- `FirstOrDefaultAsync`, `SingleOrDefaultAsync`, `AnyAsync`, `CountAsync` gibi standart LINQ semantiklerine uyum

## Onerilen sorgulama standardi

- PK ile tek kayit: `GetByIdAsync`
- Tek kayit ama first-match yeterli: `Query(...).FirstOrDefaultAsync()`
- Tek kayit ve uniqueness bekleniyor: `Query(...).SingleOrDefaultAsync()`
- Varlik kontrolu: `Query(...).AnyAsync()`
- Adet: `Query(...).CountAsync()`
- Liste: `GetListAsync(predicate)` veya `Query(...).ToListAsync()`

## Anti-pattern

Su tip kullanimlar performans riski tasir:

```csharp
var all = await repo.GetListAsync();
var item = all.FirstOrDefault(...);
```

```csharp
var all = await repo.GetListAsync();
var filtered = all.Where(...).ToList();
```

Framework bu kullanimi tamamen engellemez. Bunun yerine:

- dokumantasyon
- code review kurali
- ileride analyzer

ile dogru kullanima yonlendirme hedeflenir.

## Code Review kurali

Review'de asagidaki pattern'ler hata adayi kabul edilmelidir:

- `GetListAsync()` sonrasi `FirstOrDefault`
- `GetListAsync()` sonrasi `Single` / `SingleOrDefault`
- `GetListAsync()` sonrasi `Where`
- `GetListAsync()` sonrasi `Any`
- `GetListAsync()` sonrasi `Count`

Tek kayit, var-yok veya sayi ihtiyaci varsa bu islemler DB tarafinda calistirilmalidir.

---
## Usage

Kurulum ve kopyala-yapistir kod ornekleri icin: [ReadRepository_Usage.md](ReadRepository_Usage.md)
