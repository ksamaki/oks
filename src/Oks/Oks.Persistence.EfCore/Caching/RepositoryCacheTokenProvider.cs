using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.Primitives;
using Oks.Persistence.Abstractions.Caching;

namespace Oks.Persistence.EfCore.Caching;

public sealed class RepositoryCacheTokenProvider
    : IRepositoryCacheTokenProvider, IRepositoryCacheInvalidator
{
    private readonly ConcurrentDictionary<Type, CancellationTokenSource> _tokens = new();

    public IChangeToken GetChangeToken<TEntity>()
    {
        var source = _tokens.GetOrAdd(typeof(TEntity), _ => new CancellationTokenSource());
        return new CancellationChangeToken(source.Token);
    }

    public void Invalidate<TEntity>()
    {
        if (_tokens.TryRemove(typeof(TEntity), out var source))
        {
            try
            {
                if (!source.IsCancellationRequested)
                {
                    source.Cancel();
                }
            }
            finally
            {
                source.Dispose();
            }
        }
    }
}
