using Microsoft.EntityFrameworkCore;
using Oks.Persistence.Abstractions;
using Oks.Persistence.EfCore;

namespace Oks.Tests.TestSupport;

public class TestDbContext : OksDbContextBase
{
    public TestDbContext(
        DbContextOptions<TestDbContext> options,
        IOksUserProvider? userProvider = null)
        : base(options, userProvider)
    {
    }

    public DbSet<TestUser> Users => Set<TestUser>();
}
