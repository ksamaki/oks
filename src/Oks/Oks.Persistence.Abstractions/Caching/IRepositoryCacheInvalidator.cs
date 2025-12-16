namespace Oks.Persistence.Abstractions.Caching;

public interface IRepositoryCacheInvalidator
{
    void Invalidate<TEntity>();
}
