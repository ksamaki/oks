using System.ComponentModel.DataAnnotations.Schema;

namespace Oks.Domain.Base;

public abstract class AuditedEntity<TKey> : Entity<TKey>, IAuditedEntity
{
    public string? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }

    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public string? DeletedBy { get; set; }
    public DateTime? DeletedAt { get; set; }
    public bool IsDeleted { get; set; }

    [NotMapped]
    public bool IsAuditEnabled { get; set; } = true;

    protected AuditedEntity()
    {
    }

    protected AuditedEntity(TKey id) : base(id)
    {
    }
}
