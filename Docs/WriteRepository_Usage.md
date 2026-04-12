# Write Repository & Unit of Work - Usage

[Write Repository & Unit of Work - Description](WriteRepository_Description.md) | [Ana sayfa](../README.md)

Asagidaki adimlari kopyalayarak yazma repository katmanini transaction filtreleriyle birlikte devreye alabilirsin.

> Not: `IWriteRepository<TEntity, TKey>` zaten `IReadRepository<TEntity, TKey>` kontratini da icerir. Bir command handler hem veri okuyup hem guncelleme yapiyorsa genellikle yalnizca `IWriteRepository` enjekte etmesi yeterlidir.

## 1) Proje referanslarini ekle (`.csproj`)
```xml
<ItemGroup>
  <ProjectReference Include="..\\src\\Oks\\Oks.Domain\\Oks.Domain.csproj" />
  <ProjectReference Include="..\\src\\Oks\\Oks.Shared\\Oks.Shared.csproj" />
  <ProjectReference Include="..\\src\\Oks\\Oks.Persistence.Abstractions\\Oks.Persistence.Abstractions.csproj" />
  <ProjectReference Include="..\\src\\Oks\\Oks.Persistence.EfCore\\Oks.Persistence.EfCore.csproj" />
  <ProjectReference Include="..\\src\\Oks\\Oks.Web.Abstractions\\Oks.Web.Abstractions.csproj" />
  <ProjectReference Include="..\\src\\Oks\\Oks.Web\\Oks.Web.csproj" />
</ItemGroup>
```

## 2) DbContext ve audit
Audit icin `OksDbContextBase` kullanilir; kullanici bilgisi `IOksUserProvider` uzerinden alinir.

## 3) DI ve pipeline kurulumu
> Not: `Oks.Web` abstraction-only kurali nedeniyle `AddOksUnitOfWork()` tek basina persistence concrete implementasyonu eklemez. Ornekteki `AddOksEfCore<AppDbContext>()` (veya alternatif persistence kaydi) zorunludur.

```csharp
using Microsoft.EntityFrameworkCore;
using Oks.Persistence.EfCore.Extensions;
using Oks.Web.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddOksEfCore<AppDbContext>();
builder.Services.AddOksCurrentUserProvider();

builder.Services.AddControllers()
    .AddOksUnitOfWork()      // MVC action'larda otomatik commit
    .AddOksResultWrapping(); // MVC result wrapping

var app = builder.Build();

app.UseAuthorization();
app.MapControllers();

var api = app.MapGroup("/api")
    .AddOksUnitOfWork()      // Minimal API commit filter
    .AddOksResultWrapping(); // Minimal API result wrapping filter

api.MapPost("/products", async (Product dto, IWriteRepository<Product, Guid> write) =>
{
    await write.AddAsync(dto);
    return dto;
});

app.Run();
```

Okuma + yazma yapan bir handler/service ornegi:
```csharp
using Microsoft.EntityFrameworkCore;

public class AcceptFriendRequestCommandHandler : IRequestHandler<AcceptFriendRequestCommand, bool>
{
    private readonly IWriteRepository<Friendship, Guid> _friendshipRepository;

    public AcceptFriendRequestCommandHandler(IWriteRepository<Friendship, Guid> friendshipRepository)
    {
        _friendshipRepository = friendshipRepository;
    }

    public async Task<bool> Handle(AcceptFriendRequestCommand request, CancellationToken cancellationToken)
    {
        var friendship = await _friendshipRepository.Query(
                f => f.Id == request.RequestId && f.FriendUserId == request.ReceiverId)
            .SingleOrDefaultAsync(cancellationToken);

        if (friendship is null)
        {
            return false;
        }

        friendship.UpdateStatus(Guid.NewGuid(), true);
        _friendshipRepository.Update(friendship);

        return true;
    }
}
```

## 4) Skip transaction ornegi (Minimal API)
```csharp
api.MapPost("/products/import", async (BulkImportRequest request, IWriteRepository<Product, Guid> write) =>
{
    // ... import
    return Result.Ok("Ice aktarma tamamlandi");
})
.WithMetadata(new OksSkipTransactionAttribute());
```

> Ipucu: `AddOksUnitOfWork()` aktifken MVC action ve Minimal API endpoint'lerinde basarili isteklerde commit otomatik denenir. Davranisi endpoint/action bazinda kapatmak icin `[OksSkipTransaction]` metadata'sini kullanabilirsin.
