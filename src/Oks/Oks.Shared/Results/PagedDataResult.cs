namespace Oks.Shared.Results;

public sealed class PagedDataResult<T> : DataResult<IReadOnlyList<T>>
{
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }

    private PagedDataResult(
        bool success,
        ResultStatus status,
        IReadOnlyList<T>? data,
        int page,
        int pageSize,
        int totalCount,
        string? message = null,
        Meta? meta = null)
        : base(success, status, data, message, meta)
    {
        Page = page;
        PageSize = pageSize;
        TotalCount = totalCount;
    }

    public static PagedDataResult<T> Ok(
        IReadOnlyList<T> data,
        int page,
        int pageSize,
        int totalCount,
        string? message = null,
        Meta? meta = null)
        => new(true, ResultStatus.Ok, data, page, pageSize, totalCount, message, meta);
}