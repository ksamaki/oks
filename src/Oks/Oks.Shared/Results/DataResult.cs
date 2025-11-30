using Oks.Shared.Results;

public class DataResult<T> : Result
{
    public T? Data { get; init; }

    protected DataResult(bool success, ResultStatus status, T? data, string? message = null, Meta? meta = null)
        : base(success, status, message, meta)
    {
        Data = data;
    }

    public static DataResult<T> Ok(T data, string? message = null, Meta? meta = null)
        => new(true, ResultStatus.Ok, data, message, meta);

    public static DataResult<T> Fail(
        T? data,
        string? message = null,
        ResultStatus status = ResultStatus.BadRequest,
        Meta? meta = null)
        => new(false, status, data, message, meta);
}