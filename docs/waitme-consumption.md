# WaitMe - OKS Package Consumption

Bu doküman, WaitMe uygulamasının GitHub Packages üzerinden yayınlanan OKS NuGet paketlerini nasıl tüketeceğini gösterir.

## 1) Örnek `nuget.config`

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="github" value="https://nuget.pkg.github.com/<GITHUB_USERNAME>/index.json" />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
  </packageSources>
  <packageSourceCredentials>
    <github>
      <add key="Username" value="<GITHUB_USERNAME>" />
      <add key="ClearTextPassword" value="%GITHUB_PACKAGES_TOKEN%" />
    </github>
  </packageSourceCredentials>
</configuration>
```

## 2) `PackageReference` örnekleri

```xml
<ItemGroup>
  <PackageReference Include="Oks.Persistence.Abstractions" Version="1.0.0" />
  <PackageReference Include="Oks.Persistence.EfCore" Version="1.0.0" />
  <PackageReference Include="Oks.Web.Validation" Version="1.0.0" />
  <PackageReference Include="Oks.Logging.Abstractions" Version="1.0.0" />
  <PackageReference Include="Oks.Logging" Version="1.0.0" />
</ItemGroup>
```

## 3) `GITHUB_PACKAGES_TOKEN` notu

- Token, private package read yetkisine sahip olmalıdır (`read:packages`).
- CI/CD ortamlarında token'ı secret/env var olarak set edin, dosyaya düz metin yazmayın.
- Lokal geliştirmede (Windows):

```powershell
setx GITHUB_PACKAGES_TOKEN "<PAT_TOKEN>"
```

- Lokal geliştirmede (macOS/Linux):

```bash
export GITHUB_PACKAGES_TOKEN=<PAT_TOKEN>
```
