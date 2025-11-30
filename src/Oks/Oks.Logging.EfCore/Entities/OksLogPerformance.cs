using Oks.Logging.Abstractions.Enums;

namespace Oks.Logging.EfCore.Entities;

public class OksLogPerformance
{
    public long Id { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public OksLogLevel Level { get; set; } // Genelde Warning/Error
    public string Message { get; set; } = string.Empty;

    public string? CorrelationId { get; set; }
    public string? UserId { get; set; }
    public string? Path { get; set; }

    public long ElapsedMilliseconds { get; set; }
    public long ThresholdMilliseconds { get; set; }

    public string? ExtraDataJson { get; set; }
}