using Oks.Logging.Abstractions.Enums;

namespace Oks.Logging.EfCore.Entities;

public class OksLogCustom
{
    public long Id { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public OksLogLevel Level { get; set; } = OksLogLevel.Info;

    public string Message { get; set; } = string.Empty;

    public string? CorrelationId { get; set; }
    public string? UserId { get; set; }
    public string? ClientIp { get; set; }

    public string? ExtraDataJson { get; set; }
}