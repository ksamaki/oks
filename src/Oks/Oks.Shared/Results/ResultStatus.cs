namespace Oks.Shared.Results;

public enum ResultStatus
{
    Ok = 200,
    Created = 201,
    NoContent = 204,

    BadRequest = 400,
    ValidationError = 422,
    Unauthorized = 401,
    Forbidden = 403,
    NotFound = 404,
    Conflict = 409,
    TooManyRequests = 429,

    Error = 500                   // InternalError yerine bu kullanılacak
}