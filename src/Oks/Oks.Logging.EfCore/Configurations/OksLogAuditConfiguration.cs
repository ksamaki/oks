using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Oks.Logging.EfCore.Entities;

namespace Oks.Logging.EfCore.Configurations;

public class OksLogAuditConfiguration : IEntityTypeConfiguration<OksLogAudit>
{
    public void Configure(EntityTypeBuilder<OksLogAudit> builder)
    {
        builder.ToTable("OksLogAudit");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.EntityName).IsRequired().HasMaxLength(200);
        builder.Property(x => x.EntityId).IsRequired().HasMaxLength(200);

        builder.Property(x => x.Operation).IsRequired().HasMaxLength(50);

        builder.Property(x => x.OldValuesJson).HasMaxLength(4000);
        builder.Property(x => x.NewValuesJson).HasMaxLength(4000);

        builder.Property(x => x.UserId).HasMaxLength(100);
        builder.Property(x => x.CorrelationId).HasMaxLength(100);

        builder.Property(x => x.CreatedAtUtc).IsRequired();
    }
}