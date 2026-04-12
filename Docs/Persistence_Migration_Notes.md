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
