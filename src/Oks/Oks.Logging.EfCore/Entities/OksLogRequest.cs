using Oks.Logging.Abstractions.Enums;

namespace Oks.Logging.EfCore.Entities;

public class OksLogRequest
{
    public long Id { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public OksLogLevel Level { get; set; }
    public string Message { get; set; } = string.Empty;

    public string? CorrelationId { get; set; }
    public string? UserId { get; set; }
    public string? ClientIp { get; set; }
    public string? HttpMethod { get; set; }
    public string? Path { get; set; }
    public int? StatusCode { get; set; }

    public long? ElapsedMilliseconds { get; set; }
}