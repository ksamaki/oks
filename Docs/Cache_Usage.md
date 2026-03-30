# Cache - Usage

[Cache - Description](Cache_Description.md) | [Ana sayfa](../README.md)

## 1) DI kayıtları
> Mimari not (2026-03-30): `Oks.Web` cache filtreleri sadece abstraction kontratlarını kullanır. Bu nedenle cache provider'ı (`AddOksCaching(...)` gibi) host/integration katmanında ayrıca register edilmelidir.

```csharp
using Oks.Caching.Extensions;
using Oks.Web.Extensions;

builder.Services.AddOksCaching(options =>
{
    options.UseDistributedCache();
    options.UseRedis(builder.Configuration.GetConnectionString("Redis")!);

    options.DefaultEntryOptions.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
    options.DefaultEntryOptions.SoftExpiration = TimeSpan.FromSeconds(30);

    options.CacheAllRepositoryQueries();
});

builder.Services.AddControllers().AddOksCustomCaching();
builder.Services.AddOksCustomCaching(); // Minimal API
```

## 2) Entity-level cache
```csharp
using Oks.Caching.Abstractions;

[OksEntityCache(TtlSeconds = 300, Tags = ["user"])]
public class User : AuditedEntity<Guid>
{
}
```

## 3) Query-level cache
```csharp
[OksCache(
    Key = "friends:list:user:{userId}",
    Tags = new[] { "user-friends:{userId}" },
    TtlSeconds = 300,
    CacheEmptyResult = true,
    StampedeProtection = true
)]
Task<List<UserFriendDto>> ListUserFriends(Guid userId);
```

Aynı method farklı parametre ile çağrıldığında farklı key üretir:
- `friends:list:user:111...`
- `friends:list:user:222...`

## 4) Invalidate (dependency/tag bazlı)
```csharp
[OksCacheInvalidate(
    Tags = new[] { "user-friends:{userId}", "user-friends:{friendUserId}" }
)]
Task RemoveFriend(Guid userId, Guid friendUserId);
```

### Wildcard invalidate
```csharp
[OksCacheInvalidate(Tags = new[] { "user-friends:*" })]
Task RebuildFriendsProjection();
```

## 5) Birlikte kullanım örneği
```csharp
public interface IFriendService
{
    [OksCache(
        Key = "friends:list:user:{userId}",
        Tags = new[] { "user-friends:{userId}" },
        TtlSeconds = 300
    )]
    Task<List<UserFriendDto>> ListUserFriends(Guid userId);

    [OksCache(
        Key = "friends:summary:user:{userId}",
        Tags = new[] { "user-friends:{userId}" }
    )]
    Task<FriendSummaryDto> GetFriendSummary(Guid userId);

    [OksCacheInvalidate(
        Tags = new[] { "user-friends:{userId}", "user-friends:{friendUserId}" }
    )]
    Task AddFriend(Guid userId, Guid friendUserId);

    [OksCacheInvalidate(
        Tags = new[] { "user-friends:{userId}", "user-friends:{friendUserId}" }
    )]
    Task RemoveFriend(Guid userId, Guid friendUserId);
}
```

## 6) Kısıtlar
- `[OksEntityCache]` method üstünde kullanılamaz.
- `[OksCache]` entity class üstünde kullanılamaz.
- Yanlış kullanım compile-time’da `AttributeUsage` ile engellenir.
- Runtime’da web filter katmanında ek doğrulama yapılır.
