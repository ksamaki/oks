namespace Oks.Caching.Internal;

internal sealed class CacheLookupResult<T>
{
    public CacheLookupResult(T? value, bool shouldRefresh)
    {
        Value = value;
        ShouldRefresh = shouldRefresh;
    }

    public T? Value { get; }

    public bool ShouldRefresh { get; }
}
