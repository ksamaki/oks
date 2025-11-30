namespace Oks.Domain.Base;

public interface IAuditedEntity
{
    string? CreatedBy { get; set; }
    DateTime CreatedAt { get; set; }

    string? UpdatedBy { get; set; }
    DateTime? UpdatedAt { get; set; }

    string? DeletedBy { get; set; }
    DateTime? DeletedAt { get; set; }

    bool IsDeleted { get; set; }

    bool IsAuditEnabled { get; set; }
}