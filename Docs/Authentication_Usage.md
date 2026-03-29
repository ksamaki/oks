# Authentication - Usage

[Authentication - Description](Authentication_Description.md) | [Ana sayfa](../README.md)

Bu doküman, JWT varsayılan kurulumunu ve opsiyonel OpenIddict adaptasyonunu uçtan uca örnekler.

## 1) Proje referansları

```xml
<ItemGroup>
  <ProjectReference Include="..\\src\\Oks\\Oks.Authentication.Abstractions\\Oks.Authentication.Abstractions.csproj" />
  <ProjectReference Include="..\\src\\Oks\\Oks.Authentication.Core\\Oks.Authentication.Core.csproj" />
  <ProjectReference Include="..\\src\\Oks\\Oks.Authentication.Jwt\\Oks.Authentication.Jwt.csproj" />
  <ProjectReference Include="..\\src\\Oks\\Oks.Authentication.EntityFrameworkCore\\Oks.Authentication.EntityFrameworkCore.csproj" />
  <ProjectReference Include="..\\src\\Oks\\Oks.Authentication.AspNetCore\\Oks.Authentication.AspNetCore.csproj" />

  <!-- Opsiyonel -->
  <ProjectReference Include="..\\src\\Oks\\Oks.Authentication.OpenIddict\\Oks.Authentication.OpenIddict.csproj" />
</ItemGroup>
```

## 2) DbContext kurulumu

```csharp
using Microsoft.EntityFrameworkCore;
using Oks.Authentication.EntityFrameworkCore.Options;
using Oks.Authentication.EntityFrameworkCore.Persistence;

public sealed class AppAuthDbContext : OksAuthenticationDbContext
{
    public AppAuthDbContext(
        DbContextOptions<OksAuthenticationDbContext> options,
        OksAuthenticationEfCoreOptions efOptions)
        : base(options, efOptions)
    {
    }

    // Host uygulama isterse ek DbSet/konfigürasyonlar ekleyebilir.
}
```

## 3) Service registration (JWT default)

```csharp
using Microsoft.EntityFrameworkCore;
using Oks.Authentication.AspNetCore.Extensions;
using Oks.Authentication.EntityFrameworkCore.Extensions;
using Oks.Authentication.Jwt.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<OksAuthenticationDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services
    .AddOksAuthentication()
    .AddOksJwt(jwt =>
    {
        jwt.Issuer = "oks-auth";
        jwt.Audience = "oks-services";
        jwt.SigningKey = builder.Configuration["Auth:Jwt:SigningKey"]!;
        jwt.AccessTokenMinutes = 15;
        jwt.RefreshTokenDays = 7;
    })
    .AddOksAuthenticationEntityFramework<OksAuthenticationDbContext>(ef =>
    {
        ef.Schema = "oks_auth";
        ef.AutoMigrate = true;
    });

builder.Services.AddControllers();

var app = builder.Build();

app.UseOksAuthentication();
await app.Services.UseOksAuthenticationEntityFrameworkAsync();

app.MapControllers();
app.Run();
```

## 4) Permission policy örneği

```csharp
builder.Services.AddOksPermissionPolicy("CanManageUsers", "users.manage");

[Authorize(Policy = "CanManageUsers")]
[HttpGet("/api/admin/users")]
public IActionResult GetUsers() => Ok();
```

## 5) Login/Refresh/Logout endpoint örneği

```csharp
using Oks.Authentication.Abstractions.Contracts;
using Oks.Authentication.Abstractions.Models;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        [FromServices] IAuthenticationService auth,
        CancellationToken ct)
    {
        var tokenPair = await auth.LoginAsync(request, ct);
        return Ok(tokenPair);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(
        [FromBody] RefreshTokenRequest request,
        [FromServices] IAuthenticationService auth,
        CancellationToken ct)
    {
        var tokenPair = await auth.RefreshAsync(request, ct);
        return Ok(tokenPair);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(
        [FromBody] LogoutRequest request,
        [FromServices] IAuthenticationService auth,
        CancellationToken ct)
    {
        await auth.LogoutAsync(request, ct);
        return NoContent();
    }
}
```

## 6) Seed çalıştırma örneği

```csharp
using Oks.Authentication.EntityFrameworkCore.Services;

using var scope = app.Services.CreateScope();
var seeder = scope.ServiceProvider.GetRequiredService<OksAuthenticationSeeder>();
await seeder.SeedAsync();
```

## 7) Multi-client seed örneği

```csharp
// Örnek clientlar:
// - oks_admin_web      (SPA/Admin)
// - oks_mobile_app     (Mobile)
// - oks_internal_svc   (Service-to-Service)
// - oks_external_int   (External integration)
```

## 8) OpenIddict opsiyonel entegrasyon

Bu modül adapter pattern ile çalışır.
Host uygulama OpenIddict paketlerini kendi projesine ekleyip konfigüratör sınıfı sağlar.

```csharp
using Oks.Authentication.OpenIddict.Options;
using Oks.Authentication.OpenIddict.Services;

public sealed class MyOpenIddictConfigurator : IOksOpenIddictConfigurator
{
    public void Configure(IServiceCollection services, OksOpenIddictOptions options)
    {
        // Burada host uygulama OpenIddict çağrılarını yapar.
        // services.AddOpenIddict()....
    }
}

builder.Services.AddOksOpenIddict<MyOpenIddictConfigurator>(opt =>
{
    // grant/scope ayarları
});
```

## 9) Güvenlik checklist (önerilen)

- JWT signing key'i appsettings yerine secret manager / vault üzerinden yönetin.
- Refresh token ham değerini loglamayın.
- Failed login denemelerini `OksLoginAttempt` ile izleyin.
- Revoke edilen token/session için API seviyesinde deny-list stratejisi planlayın.
- Üretimde HTTPS zorunlu + secure cookie/header policy kullanın.

## 10) Hızlı özet akış

1. `AddOksAuthentication` + `AddOksJwt` + `AddOksAuthenticationEntityFramework`
2. `UseOksAuthentication`
3. (Opsiyonel) `UseOksAuthenticationEntityFrameworkAsync` ile migrate
4. (Opsiyonel) `OksAuthenticationSeeder` ile başlangıç role/permission/client seed
5. Controller/endpoint üzerinden `IAuthenticationService` çağrıları
