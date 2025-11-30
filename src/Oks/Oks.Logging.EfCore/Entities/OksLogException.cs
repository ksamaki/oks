using Oks.Logging.Abstractions.Enums;

namespace Oks.Logging.EfCore.Entities;

public class OksLogException
{
    public long Id { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public OksLogLevel Level { get; set; } = OksLogLevel.Error;

    public string Message { get; set; } = string.Empty;
    public string? Exception { get; set; }

    public string? Path { get; set; }
    public string? HttpMethod { get; set; }
    public string? StatusCode { get; set; }

    public string? CorrelationId { get; set; }
    public string? UserId { get; set; }
    public string? ClientIp { get; set; }
}