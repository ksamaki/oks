using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Oks.Logging.EfCore.Entities;

namespace Oks.Logging.EfCore.Configurations;

public class OksLogPerformanceConfiguration : IEntityTypeConfiguration<OksLogPerformance>
{
    public void Configure(EntityTypeBuilder<OksLogPerformance> builder)
    {
        builder.ToTable("OksLogPerformance");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Message)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(x => x.Path).HasMaxLength(500);
        builder.Property(x => x.CorrelationId).HasMaxLength(100);
        builder.Property(x => x.UserId).HasMaxLength(100);

        builder.Property(x => x.ExtraDataJson).HasMaxLength(4000);

        builder.Property(x => x.CreatedAtUtc).IsRequired();
    }
}