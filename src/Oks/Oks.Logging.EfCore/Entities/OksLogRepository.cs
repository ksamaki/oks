using Oks.Logging.Abstractions.Enums;

namespace Oks.Logging.EfCore.Entities;

public class OksLogRepository
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public DateTime CreatedAtUtc { get; set; }

    public OksLogLevel Level { get; set; } = OksLogLevel.Info;
    public string Message { get; set; } = string.Empty;

    public string? CorrelationId { get; set; }
    public string? UserId { get; set; }

    public string EntityName { get; set; } = string.Empty;
    public string OperationType { get; set; } = string.Empty; // "Read" / "Write"

    public long? ElapsedMilliseconds { get; set; }

    public string? ExtraDataJson { get; set; }
}
