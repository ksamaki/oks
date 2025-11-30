using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Oks.Logging.EfCore.Entities;

namespace Oks.Logging.EfCore.Configurations;

public class OksLogRepositoryConfiguration : IEntityTypeConfiguration<OksLogRepository>
{
    public void Configure(EntityTypeBuilder<OksLogRepository> builder)
    {
        builder.ToTable("OksLogRepository");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Message)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(x => x.EntityName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.OperationType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.CorrelationId).HasMaxLength(100);
        builder.Property(x => x.UserId).HasMaxLength(100);

        builder.Property(x => x.ExtraDataJson).HasMaxLength(4000);
        builder.Property(x => x.CreatedAtUtc).IsRequired();
    }
}