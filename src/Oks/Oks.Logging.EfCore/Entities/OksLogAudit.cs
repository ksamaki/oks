using Oks.Logging.Abstractions.Enums;

namespace Oks.Logging.EfCore.Entities;

public class OksLogAudit
{
    public long Id { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public string EntityName { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;

    public string Operation { get; set; } = string.Empty;
    // "Create", "Update", "Delete", "SoftDelete", "Recover"

    public string? OldValuesJson { get; set; }
    public string? NewValuesJson { get; set; }

    public string? UserId { get; set; }
    public string? CorrelationId { get; set; }
}