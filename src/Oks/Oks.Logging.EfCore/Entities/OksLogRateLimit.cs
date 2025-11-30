using Oks.Logging.Abstractions.Enums;

namespace Oks.Logging.EfCore.Entities;

public class OksLogRateLimit
{
    public long Id { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public OksLogLevel Level { get; set; } // Info/Warning/Error
    public string Message { get; set; } = string.Empty;

    public string? CorrelationId { get; set; }
    public string? UserId { get; set; }
    public string? ClientIp { get; set; }
    public string? Path { get; set; }
    public string? HttpMethod { get; set; }

    public int? StatusCode { get; set; } // Genelde 429 veya null (skip durumunda bile loglar)

    public int? RequestsPerMinuteLimit { get; set; }
    public int? CurrentCount { get; set; }

    public string? ExtraDataJson { get; set; }
}