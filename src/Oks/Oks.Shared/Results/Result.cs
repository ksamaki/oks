namespace Oks.Shared.Results;

public class Result
{
    public bool Success { get; init; }
    public string? Message { get; init; }
    public ResultStatus Status { get; init; }
    public Meta? Meta { get; init; }

    protected Result(bool success, ResultStatus status, string? message = null, Meta? meta = null)
    {
        Success = success;
        Status = status;
        Message = message;
        Meta = meta;
    }

    public static Result Ok(string? message = null, Meta? meta = null)
        => new(true, ResultStatus.Ok, message, meta);

    public static Result Fail(string? message = null, ResultStatus status = ResultStatus.BadRequest, Meta? meta = null)
        => new(false, status, message, meta);
}