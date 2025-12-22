# Cache - Usage

[Cache - Description](Cache_Description.md) | [Ana sayfa](../README.md)

Cache özelliğini projene eklemek için aşağıdaki adımları uygulayabilirsin.

## 1) Proje referansları (`.csproj`)
```xml
<ItemGroup>
  <ProjectReference Include="..\src\Oks\Oks.Domain\Oks.Domain.csproj" />
  <ProjectReference Include="..\src\Oks\Oks.Shared\Oks.Shared.csproj" />
  <ProjectReference Include="..\src\Oks\Oks.Persistence.EfCore\Oks.Persistence.EfCore.csproj" />
  <ProjectReference Include="..\src\Oks\Oks.Caching.Abstractions\Oks.Caching.Abstractions.csproj" />
  <ProjectReference Include="..\src\Oks\Oks.Caching\Oks.Caching.csproj" />
  <ProjectReference Include="..\src\Oks\Oks.Caching.Redis\Oks.Caching.Redis.csproj" /> <!-- Redis opsiyonel -->
  <ProjectReference Include="..\src\Oks\Oks.Web.Abstractions\Oks.Web.Abstractions.csproj" />
  <ProjectReference Include="..\src\Oks\Oks.Web\Oks.Web.csproj" />
</ItemGroup>
```

(...content unchanged until Redis provider section...)

## 6) Redis provider'ını etkinleştirme
```csharp
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "oks:";
});

builder.Services.AddOksCaching(caching =>
{
    caching.UseDistributedCache(); // Redis için IDistributedCache kullan
    caching.AddReadRepositoryCaching();
});
```

> NOTE: Mevcut implementasyonda tag-index (ICacheTagIndex) varsayılan olarak InMemoryCacheTagIndex'dir. Dağıtık/çoklu node (Redis) senaryolarında tag-index'in merkezi (örn. Redis setleri) bir implementasyonu gereklidir. InMemoryCacheTagIndex yalnızca tek-node veya test senaryoları içindir.

(...rest unchanged...)
