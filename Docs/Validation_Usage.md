# Validation (FluentValidation) - Usage

[Validation - Description](Validation_Description.md) | [Ana sayfa](../README.md)

## 1) Proje referansları
```xml
<ItemGroup>
  <ProjectReference Include="..\\src\\Oks\\Oks.Shared\\Oks.Shared.csproj" />
  <ProjectReference Include="..\\src\\Oks\\Oks.Web.Abstractions\\Oks.Web.Abstractions.csproj" />
  <ProjectReference Include="..\\src\\Oks\\Oks.Web\\Oks.Web.csproj" />
  <ProjectReference Include="..\\src\\Oks\\Oks.Web.Validation\\Oks.Web.Validation.csproj" />
</ItemGroup>
```

## 2) DI kaydı (MVC + Minimal API + MediatR)
```csharp
using Oks.Web.Extensions;
using Oks.Web.Validation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddOksResultWrapping()
    .AddOksFluentValidation(typeof(Program).Assembly);

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
builder.Services.AddOksMediatRValidationBehavior();

var app = builder.Build();

var api = app.MapGroup("/api")
    .AddOksValidation()
    .AddOksResultWrapping();

app.MapControllers();
app.Run();
```

## 3) FluentValidation validator örneği
```csharp
public sealed class CreateProductValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Price).GreaterThan(0);
    }
}
```

## 4) MVC skip örneği
```csharp
[HttpPost("skip")]
[OksSkipValidation]
public IActionResult CreateWithoutValidation(CreateProductRequest dto) => Ok(dto);
```

## 5) Minimal API skip örneği
```csharp
api.MapPost("/products/skip", (CreateProductRequest dto) => dto)
   .WithMetadata(new OksSkipValidationAttribute());
```

## 6) MediatR skip örneği
```csharp
[OksSkipValidation]
public sealed record ImportCatalogCommand(string Source) : IRequest<Result>;
```

Not: MediatR tarafında ilgili request için `IValidator<TRequest>` kayıtlı değilse behavior otomatik olarak `next()` çağırır.
