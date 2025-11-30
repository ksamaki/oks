using Microsoft.EntityFrameworkCore;
using Oks.Persistence.EfCore;

namespace Oks.Tests.TestSupport;

public class TestDbContext : OksDbContextBase
{
    public TestDbContext(DbContextOptions<TestDbContext> options)
        : base(options)
    {
    }

    public DbSet<TestUser> Users => Set<TestUser>();

    protected override string? GetCurrentUserIdentifier()
        => "test-user";
}