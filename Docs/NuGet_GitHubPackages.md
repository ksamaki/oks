# Publishing OKS NuGet Packages to GitHub Packages

This guide shows how to build, pack, publish, and consume the OKS framework packages from GitHub Packages.

## 1. Repository structure

The solution under `src/Oks/Oks.slnx` contains multiple packable class library projects, including:

- `Oks.Domain`
- `Oks.Shared`
- `Oks.Persistence.Abstractions`
- `Oks.Persistence.EfCore`
- `Oks.Logging.Abstractions`
- `Oks.Logging.EfCore`
- `Oks.Logging`
- `Oks.Caching.Abstractions`
- `Oks.Caching`
- `Oks.Web.Abstractions`
- `Oks.Web.Validation`
- `Oks.Web`
- `Oks.RealTime.SignalR`

All library projects are configured to produce NuGet packages with a shared version, author, repository URL, symbols package, and README file.

## 2. NuGet metadata in each package

Each library project now includes:

- `PackageId`
- `Description`
- `IsPackable=true`

Shared package metadata is centralized in `Directory.Build.props`:

- `Version`
- `Authors`
- `RepositoryUrl`
- `RepositoryType`
- `PackageProjectUrl`
- `PackageReadmeFile`
- `IncludeSymbols`
- `SymbolPackageFormat`

## 3. Build the solution

```bash
dotnet restore src/Oks/Oks.slnx
dotnet build src/Oks/Oks.slnx --configuration Release
```

## 4. Pack all projects into `.nupkg` files

```bash
dotnet pack src/Oks/Oks.slnx   --configuration Release   --no-build   --output ./artifacts/packages
```

If you want to override the package version during packing:

```bash
dotnet pack src/Oks/Oks.slnx   --configuration Release   --no-build   -p:PackageVersion=1.0.0   --output ./artifacts/packages
```

## 5. Configure the GitHub Packages NuGet source

### Option A: add the source with the .NET CLI

```bash
dotnet nuget add source   --username ksamaki   --password <GITHUB_PAT>   --store-password-in-clear-text   --name github   https://nuget.pkg.github.com/ksamaki/index.json
```

Required PAT scopes:

- `read:packages` to install packages
- `write:packages` to publish packages
- `repo` if the repository or package is private

### Option B: use `nuget.config`

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="github" value="https://nuget.pkg.github.com/ksamaki/index.json" />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
  </packageSources>
  <packageSourceCredentials>
    <github>
      <add key="Username" value="ksamaki" />
      <add key="ClearTextPassword" value="%GITHUB_PACKAGES_TOKEN%" />
    </github>
  </packageSourceCredentials>
</configuration>
```

## 6. Publish packages to GitHub Packages

After packing, publish all packages with:

```bash
dotnet nuget push "./artifacts/packages/*.nupkg"   --source github   --api-key <GITHUB_PAT>   --skip-duplicate
```

If you prefer publishing directly to the feed URL:

```bash
dotnet nuget push "./artifacts/packages/*.nupkg"   --source https://nuget.pkg.github.com/ksamaki/index.json   --api-key <GITHUB_PAT>   --skip-duplicate
```

## 7. Consume packages from another project

### Install a package with the .NET CLI

```bash
dotnet add package Oks.Core
```

Because the current repository contains packages such as `Oks.Domain`, `Oks.Shared`, `Oks.Persistence.Abstractions`, and `Oks.Logging`, a real install command would typically look like this:

```bash
dotnet add package Oks.Domain --version 1.0.0
dotnet add package Oks.Persistence.Abstractions --version 1.0.0
dotnet add package Oks.Persistence.EfCore --version 1.0.0
```

### Add the package source before restore

```bash
dotnet nuget add source   --username ksamaki   --password <GITHUB_PAT>   --store-password-in-clear-text   --name github   https://nuget.pkg.github.com/ksamaki/index.json

dotnet restore
```

## 8. Troubleshooting

### 401 Unauthorized

Common causes:

- The PAT does not include `read:packages` or `write:packages`.
- The token belongs to a different GitHub user than the username configured in NuGet.
- The repository is private and the token does not include `repo` access.
- The `GITHUB_TOKEN` is being used outside GitHub Actions.

Checks:

```bash
dotnet nuget list source
```

```bash
dotnet nuget remove source github
dotnet nuget add source   --username ksamaki   --password <GITHUB_PAT>   --store-password-in-clear-text   --name github   https://nuget.pkg.github.com/ksamaki/index.json
```

### No packages found

Common causes:

- The package source URL is incorrect.
- The package name or version is wrong.
- The package is private and the authenticated user does not have access.
- Local caches are stale.

Checks:

```bash
dotnet nuget locals all --clear
dotnet restore --force --no-cache
```

Verify the package ID exactly matches the `.csproj` `PackageId`, for example `Oks.Domain` or `Oks.Persistence.EfCore`.

### Package not visible in GitHub

Common causes:

- The workflow has not run on `main` yet.
- The push used a duplicate version and GitHub skipped it.
- The job lacked `packages: write` permission.
- The package was published under a different owner/feed URL.

Checks:

```bash
gh run list --workflow publish.yml
```

```bash
dotnet nuget push "./artifacts/packages/*.nupkg"   --source https://nuget.pkg.github.com/ksamaki/index.json   --api-key <GITHUB_PAT>   --skip-duplicate
```

## 9. GitHub Actions automation

The repository includes `.github/workflows/publish.yml`, which automatically:

1. Runs on every push to `main`
2. Restores dependencies
3. Builds the solution in Release mode
4. Packs all library projects
5. Publishes packages to GitHub Packages

It generates a CI package version using the base `<Version>` from `Directory.Build.props` and appends `-ci.<run_number>` so every main branch push produces a unique package version.

Example generated version:

```text
1.0.0-ci.27
```

## 10. Suggested release strategy

- Use `main` pushes for CI/prerelease packages such as `1.0.0-ci.27`.
- Use manual version bumps in `Directory.Build.props` for stable releases.
- Consume stable versions explicitly in downstream applications.
