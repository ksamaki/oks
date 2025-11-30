using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Oks.Logging.EfCore.Entities;

namespace Oks.Logging.EfCore.Configurations;

public class OksLogCustomConfiguration : IEntityTypeConfiguration<OksLogCustom>
{
    public void Configure(EntityTypeBuilder<OksLogCustom> builder)
    {
        builder.ToTable("OksLogCustom");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Message)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(x => x.CorrelationId).HasMaxLength(100);
        builder.Property(x => x.UserId).HasMaxLength(100);
        builder.Property(x => x.ClientIp).HasMaxLength(50);

        builder.Property(x => x.ExtraDataJson).HasMaxLength(4000);
        builder.Property(x => x.CreatedAtUtc).IsRequired();
    }
}