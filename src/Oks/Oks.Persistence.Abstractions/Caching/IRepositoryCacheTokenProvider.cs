using Microsoft.Extensions.Primitives;

namespace Oks.Persistence.Abstractions.Caching;

public interface IRepositoryCacheTokenProvider
{
    IChangeToken GetChangeToken<TEntity>();
}
