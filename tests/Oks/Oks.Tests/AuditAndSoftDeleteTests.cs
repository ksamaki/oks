using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Oks.Persistence.Abstractions.Repositories;
using Oks.Persistence.EfCore;
using Oks.Persistence.EfCore.Repositories;
using Oks.Tests.TestSupport;

namespace Oks.Tests;

public class AuditAndSoftDeleteTests
{
    private static TestDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new TestDbContext(options);
    }

    [Fact]
    public async Task When_Entity_Added_Audit_Fields_Should_Be_Filled()
    {
        using var context = CreateInMemoryContext();
        IWriteRepository<TestUser, int> writeRepo =
            new EfWriteRepository<TestUser, int>(context);
        IUnitOfWork uow = new EfUnitOfWork(context);

        new EfWriteRepository<TestUser, int>(context);
        var user = new TestUser { Name = "Yeni Kullanici" };
        await writeRepo.AddAsync(user);
        await uow.SaveChangesAsync();

        user.CreatedBy.Should().Be("test-user");
        user.IsDeleted.Should().BeFalse();

        user.UpdatedAt.Should().BeNull();
        user.DeletedAt.Should().BeNull();
    }

    [Fact]
    public async Task When_Entity_SoftDeleted_It_Should_Not_Appear_In_Queries()
    {
        using var context = CreateInMemoryContext();

        IWriteRepository<TestUser, int> writeRepo =
            new EfWriteRepository<TestUser, int>(context);
        IReadRepository<TestUser, int> readRepo =
            new EfReadRepository<TestUser, int>(context);
        IUnitOfWork uow = new EfUnitOfWork(context);

        var user = new TestUser { Name = "Silinecek" };

        await writeRepo.AddAsync(user);
        await uow.SaveChangesAsync();

        // soft delete
        writeRepo.Remove(user);
        await uow.SaveChangesAsync();

        user.IsDeleted.Should().BeTrue();
        user.DeletedAt.Should().NotBeNull();
        user.DeletedBy.Should().Be("test-user");

        var list = await readRepo.GetListAsync();
        list.Should().BeEmpty();
    }
}