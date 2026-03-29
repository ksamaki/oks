# Redis - Usage

[Redis - Description](Redis_Description.md) | [Ana sayfa](../README.md)

Bu doküman, `Oks.Caching` ile Redis kullanımını adım adım örnekler.

## 1) Proje referansları

```xml
<ItemGroup>
  <ProjectReference Include="..\\src\\Oks\\Oks.Caching.Abstractions\\Oks.Caching.Abstractions.csproj" />
  <ProjectReference Include="..\\src\\Oks\\Oks.Caching\\Oks.Caching.csproj" />
</ItemGroup>
```

## 2) En kısa kurulum (`UseRedis`)

```csharp
using Oks.Caching.Extensions;

builder.Services.AddOksCaching(options =>
{
    options.UseRedis(
        configuration: builder.Configuration.GetConnectionString("Redis")!,
        instanceName: "oks:");

    options.AddReadRepositoryCaching();
    options.CacheOnlyRepositoryListQueries();
});
```

## 3) Gelişmiş kurulum (`UseDistributedCache`)

```csharp
using Oks.Caching.Extensions;

builder.Services.AddOksCaching(options =>
{
    options.UseDistributedCache(services =>
    {
        services.AddStackExchangeRedisCache(redis =>
        {
            redis.Configuration = builder.Configuration["Redis:Configuration"];
            redis.InstanceName = builder.Configuration["Redis:InstanceName"] ?? "oks:";
        });
    });

    options.DefaultEntryOptions.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(3);
    options.DefaultEntryOptions.SoftExpiration = TimeSpan.FromSeconds(20);
});
```

## 4) Ortama göre provider seçimi

```csharp
builder.Services.AddOksCaching(options =>
{
    var redisConn = builder.Configuration.GetConnectionString("Redis");

    if (!string.IsNullOrWhiteSpace(redisConn))
        options.UseRedis(redisConn, "oks:prod:");
    else
        options.UseMemoryCache();
});
```

## 5) Repository cache davranışı

```csharp
builder.Services.AddOksCaching(options =>
{
    options.UseRedis(builder.Configuration.GetConnectionString("Redis")!);

    // sadece liste sorguları cache (varsayılan)
    options.CacheOnlyRepositoryListQueries();

    // veya tüm repository sorguları
    // options.CacheAllRepositoryQueries();
});
```

## 6) Operasyon önerileri

- Key prefix’i (`InstanceName`) ortam bazlı ayarlayın: `oks:dev:`, `oks:stg:`, `oks:prod:`
- Büyük payload’lar için serialization ve TTL’i dikkatli seçin.
- Cache invalidation için entity tag stratejisini (`[Cacheable]`, `[CustomCache]`) net tanımlayın.
