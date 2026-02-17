# Validation (FluentValidation) - Usage

[Validation - Description](Validation_Description.md) | [Ana sayfa](../README.md)

Validator'ları otomatik çalıştırmak için aşağıdaki adımları kopyalayabilirsin.

## 1) Proje referansları (`.csproj`)
```xml
<ItemGroup>
  <ProjectReference Include="..\\src\\Oks\\Oks.Shared\\Oks.Shared.csproj" />
  <ProjectReference Include="..\\src\\Oks\\Oks.Web.Abstractions\\Oks.Web.Abstractions.csproj" />
  <ProjectReference Include="..\\src\\Oks\\Oks.Web\\Oks.Web.csproj" />
  <ProjectReference Include="..\\src\\Oks\\Oks.Web.Validation\\Oks.Web.Validation.csproj" />
</ItemGroup>
```

## 2) DI kaydı
```csharp
using Oks.Web.Extensions;
using Oks.Web.Validation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddOksResultWrapping() // Opsiyonel ama hata formatı için önerilir
    .AddOksFluentValidation(typeof(Program).Assembly);

var app = builder.Build();

app.UseAuthorization();
app.MapControllers();

app.Run();
```

## 3) Validator ve controller örneği
```csharp
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Oks.Web.Validation.Attributes;

public class CreateProductValidator : AbstractValidator<Product>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Price).GreaterThan(0);
    }
}

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    [HttpPost]
    public IActionResult Create(Product dto)
    {
        return Ok(dto); // Geçersiz ise OksValidationFilter otomatik 400 döner
    }

    [HttpPost("skip")]
    [OksSkipValidation]
    public IActionResult CreateWithoutValidation(Product dto)
    {
        return Ok(dto); // Bu action'da validation çalışmaz
    }
}
```

Bu entegrasyon ile validator'lar otomatik bulunur, kopyala-yapıştır setup sonrası ek yapılandırma gerekmez.
