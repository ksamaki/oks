# Persistence Migration Notes

## 1) IReadRepository sorgulama standardi

Guncel API:

- `GetByIdAsync(TKey id, CancellationToken cancellationToken = default)`
- `GetListAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default)`
- `Query()`
- `Query(Expression<Func<TEntity, bool>> predicate)`

`GetAsync` repository API'sinden cikarildi.

Gerekce:

- `GetAsync` ismi first mi single mi belirsizdi
- buyuk framework mantigina daha yakin olan yaklasim, `IQueryable` uzerinden acik semantik kullanmaktir
- tek kayit / cok kayit / var-yok / sayi ihtiyaclari LINQ async operatorleriyle acik bicimde ifade edilmelidir

Onerilen kullanim:

```csharp
var first = await readRepo.Query(x => x.Email == email)
    .FirstOrDefaultAsync();

var single = await readRepo.Query(x => x.ExternalId == externalId)
    .SingleOrDefaultAsync();

var exists = await readRepo.Query(x => x.Email == email)
    .AnyAsync();

var count = await readRepo.Query(x => x.IsActive)
    .CountAsync();
```

Anti-pattern:

```csharp
var all = await readRepo.GetListAsync();
var item = all.FirstOrDefault(x => x.Id == id);
```

## 2) OksDbContextBase current user degisikligi

Eski yaklasim: `GetCurrentUserIdentifier` override.

Yeni yaklasim: `IOksUserProvider` uzerinden DI.

- Web: `builder.Services.AddOksCurrentUserProvider();`
- Worker/Console: ekstra kayit gerekmez, `NullOksUserProvider` fallback calisir.
- Minimal API + MVC senaryolarinda ayni provider calisir.

Gecis adimi:

1. `OksDbContextBase` turevlerinden `GetCurrentUserIdentifier` override'ini kaldir.
2. Web projelerinde `AddOksCurrentUserProvider()` ekle.
3. Testlerde `IOksUserProvider` mock/fake enjekte et.

## 3) Logging initial migration guncellemesi

Logging tablolarinda buyuyebilen payload alanlari icin `varchar(4000)` / `nvarchar(4000)` / `character varying(4000)` gibi sinirli kolonlar kullanilmamali.

Gerekce:

- `Exception` stack trace ve inner exception zinciri 4000 karakteri rahatlikla asabilir.
- `ExtraDataJson`, `OldValuesJson` ve `NewValuesJson` alanlari audit veya request baglaminda degiskendir; sabit uzunluk varsayimi dogru degildir.
- PostgreSQL destegi icin provider-specific tip dayatmak yerine EF tarafinda max length vermemek daha dogrudur.

Initial migration uretilirken asagidaki kolonlar sinirsiz string olarak tanimlanmali:

- `OksLogException.Exception`
- `OksLogCustom.ExtraDataJson`
- `OksLogPerformance.ExtraDataJson`
- `OksLogRateLimit.ExtraDataJson`
- `OksLogRepository.ExtraDataJson`
- `OksLogAudit.OldValuesJson`
- `OksLogAudit.NewValuesJson`

Beklenen sonuc:

- SQL Server: `nvarchar(max)`
- PostgreSQL: `text`
- Diger provider'lar: provider'in unbounded string karsiligi

Not:

- Bu alanlarda `HasMaxLength(4000)` verilmemelidir.
- Host uygulama kendi migration'ini uretiyorsa yeni initial migration veya ilgili alter migration bu modele gore alinmalidir.
