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
  <ProjectReference Include="..\src\Oks\Oks.Web.Abstractions\Oks.Web.Abstractions.csproj" />
  <ProjectReference Include="..\src\Oks\Oks.Web\Oks.Web.csproj" />
</ItemGroup>
```

> `Oks.Caching.Redis` isminde ayrı bir proje/paket yoktur. Dağıtık cache için uygulama tarafında standart `IDistributedCache` sağlayıcısını (ör. `AddStackExchangeRedisCache`) kaydetmen yeterlidir.

## 2) Namespace'ler
```csharp
using Oks.Caching.Extensions;
using Oks.Persistence.EfCore;
```

`AddOksCaching(...)`, `UseDistributedCache()` ve `AddReadRepositoryCaching()` extension method'ları `Oks.Caching.Extensions` namespace'i altındadır.

(...content unchanged until Redis provider section...)

## 6) Redis / IDistributedCache provider'ını etkinleştirme
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

Alternatif olarak varsayılan memory cache davranışı için sadece aşağıdaki kayıt yeterlidir:

```csharp
builder.Services.AddOksCaching(caching =>
{
    caching.AddReadRepositoryCaching();
});
```

> NOTE: Mevcut implementasyonda tag-index (`ICacheTagIndex`) varsayılan olarak `InMemoryCacheTagIndex`'dir. Dağıtık/çoklu node (Redis) senaryolarında tag-index'in merkezi (örn. Redis setleri) bir implementasyonu gereklidir. `InMemoryCacheTagIndex` yalnızca tek-node veya test senaryoları içindir.

(...rest unchanged...)
