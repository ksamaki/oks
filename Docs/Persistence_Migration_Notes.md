# Persistence Migration Notes

## 1) IReadRepository yeni overload
Yeni eklenen API:
- `GetAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)`
- `GetListAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default)` (mevcut davranış korunur)

Notlar:
- `GetByIdAsync` kaldırılmadı, geriye uyumluluk korunur.
- Predicate tabanlı filtreleme EF Core tarafından SQL'e çevrilecek şekilde tasarlandı.

## 2) OksDbContextBase current user değişikliği
Eski yaklaşım: `GetCurrentUserIdentifier` override.

Yeni yaklaşım: `IOksUserProvider` üzerinden DI.
- Web: `builder.Services.AddOksCurrentUserProvider();`
- Worker/Console: ekstra kayıt gerekmez, `NullOksUserProvider` fallback çalışır.
- Minimal API + MVC senaryolarında aynı provider çalışır (IHttpContextAccessor üzerinden).

Geçiş adımı:
1. `OksDbContextBase` türevlerinden `GetCurrentUserIdentifier` override'ını kaldır.
2. Web projelerinde `AddOksCurrentUserProvider()` ekle.
3. Testlerde `IOksUserProvider` mock/fake enjekte et.
