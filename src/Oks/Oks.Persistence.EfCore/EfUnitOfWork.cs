using Microsoft.EntityFrameworkCore;
using Oks.Persistence.Abstractions.Repositories;

namespace Oks.Persistence.EfCore;

public class EfUnitOfWork : IUnitOfWork
{
    private readonly DbContext _dbContext;

    public EfUnitOfWork(DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Değişiklik yoksa DB'ye gitmeye gerek yok
        if (!_dbContext.ChangeTracker.HasChanges())
            return Task.FromResult(0);

        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
