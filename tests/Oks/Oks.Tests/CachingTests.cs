using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Oks.Caching;
using Oks.Caching.Abstractions;
using Oks.Caching.Extensions;
using Oks.Caching.Tags;
using Oks.Persistence.Abstractions.Repositories;
using Oks.Persistence.EfCore;
using Oks.Tests.TestSupport;

namespace Oks.Tests;

public class CachingTests
{
    [Fact]
    public void OksCachingOptions_Fluent_Methods_Should_Be_Available()
    {
        var options = new OksCachingOptions();

        options.UseDistributedCache()
               .AddReadRepositoryCaching();

        options.Provider.Should().Be(CacheProvider.Distributed);
        options.RepositoryCachingEnabled.Should().BeTrue();
    }

    [Fact]
    public async Task CacheService_Should_Remove_By_Tag()
    {
        var service = new CacheService(
            new MemoryCache(new MemoryCacheOptions()),
            distributedCache: null,
            new DefaultCacheSerializer(),
            new InMemoryCacheTagIndex(),
            Options.Create(new OksCachingOptions()));

        var key = new CacheKey(new[] { "oks", "cache", "user", "1" });
        await service.SetAsync(key, "first", new CacheEntryOptions { Tags = new[] { "User:1" } });

        var cached = await service.GetAsync<string>(key);
        cached.Should().Be("first");

        await service.RemoveByTagAsync("User:1");
        var afterEvict = await service.GetAsync<string>(key);
        afterEvict.Should().BeNull();
    }

    [Fact]
    public async Task Repositories_Should_Cache_Reads_And_Evict_On_Write()
    {
        var services = new ServiceCollection();
        services.AddDbContext<TestDbContext>(o => o.UseInMemoryDatabase(Guid.NewGuid().ToString()));
        services.AddOksEfCore<TestDbContext>();
        services.AddOksCachingWithRepositories(o =>
        {
            o.DefaultEntryOptions.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
            o.DefaultEntryOptions.SoftExpiration = TimeSpan.FromSeconds(5);
        });

        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        var sp = scope.ServiceProvider;

        var writeRepo = sp.GetRequiredService<IWriteRepository<TestUser, int>>();
        var readRepo = sp.GetRequiredService<IReadRepository<TestUser, int>>();
        var uow = sp.GetRequiredService<IUnitOfWork>();

        await writeRepo.AddAsync(new TestUser { Name = "Cached" });
        await uow.SaveChangesAsync();

        var first = await readRepo.GetByIdAsync(1);
        first!.Name.Should().Be("Cached");

        var context = sp.GetRequiredService<TestDbContext>();
        var user = await context.Users.FirstAsync();
        user.Name = "Stale";
        await context.SaveChangesAsync();

        var stillCached = await readRepo.GetByIdAsync(1);
        stillCached!.Name.Should().Be("Cached");

        user.Name = "Updated";
        await context.SaveChangesAsync();
        await writeRepo.UpdateAsync(user);
        await uow.SaveChangesAsync();

        var refreshed = await readRepo.GetByIdAsync(1);
        refreshed!.Name.Should().Be("Updated");
    }

    [Fact]
    [Cacheable(KeyTemplate= "tests:user:{id}", DurationSeconds= 30, Tags = new[] { "feature" })]
    public async Task CacheableAttribute_Should_Influence_Cache()
    {
        var services = new ServiceCollection();
        services.AddDbContext<TestDbContext>(o => o.UseInMemoryDatabase(Guid.NewGuid().ToString()));
        services.AddOksEfCore<TestDbContext>();
        services.AddOksCaching();

        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        var sp = scope.ServiceProvider;

        var writeRepo = sp.GetRequiredService<IWriteRepository<TestUser, int>>();
        var readRepo = sp.GetRequiredService<IReadRepository<TestUser, int>>();
        var uow = sp.GetRequiredService<IUnitOfWork>();
        var tags = sp.GetRequiredService<ICacheTagIndex>();

        await writeRepo.AddAsync(new TestUser { Name = "Initial" });
        await uow.SaveChangesAsync();

        var cached = await readRepo.GetByIdAsync(1);
        cached.Should().NotBeNull();

        tags.KeysFor("feature").Should().NotBeEmpty();
        tags.KeysFor(nameof(TestUser)).Should().NotBeEmpty();

        cached!.Name = "Changed";
        await writeRepo.UpdateAsync(cached);
        await uow.SaveChangesAsync();

        tags.KeysFor("feature").Should().BeEmpty();
        tags.KeysFor(nameof(TestUser)).Should().BeEmpty();
    }

    [Fact]
    [Cacheable(DurationSeconds = 60, Tags = new[] { "flush" })]
    [CacheEvict(Tags = new[] { "flush" }, EvictAllEntityCache = true)]
    public async Task CacheEvictAttribute_Should_Remove_Custom_Tags()
    {
        var services = new ServiceCollection();
        services.AddDbContext<TestDbContext>(o => o.UseInMemoryDatabase(Guid.NewGuid().ToString()));
        services.AddOksEfCore<TestDbContext>();
        services.AddOksCachingWithRepositories();

        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        var sp = scope.ServiceProvider;

        var writeRepo = sp.GetRequiredService<IWriteRepository<TestUser, int>>();
        var readRepo = sp.GetRequiredService<IReadRepository<TestUser, int>>();
        var uow = sp.GetRequiredService<IUnitOfWork>();
        var tags = sp.GetRequiredService<ICacheTagIndex>();

        await writeRepo.AddAsync(new TestUser { Name = "Initial" });
        await uow.SaveChangesAsync();

        var cached = await readRepo.GetByIdAsync(1);
        cached.Should().NotBeNull();

        tags.KeysFor("flush").Should().NotBeEmpty();

        cached!.Name = "Changed";
        await writeRepo.UpdateAsync(cached);
        await uow.SaveChangesAsync();

        tags.KeysFor("flush").Should().BeEmpty();
        tags.KeysFor(nameof(TestUser)).Should().BeEmpty();
    }
}
