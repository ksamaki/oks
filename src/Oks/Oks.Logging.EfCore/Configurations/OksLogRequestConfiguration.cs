using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Oks.Logging.EfCore.Entities;

namespace Oks.Logging.EfCore.Configurations;

public class OksLogRequestConfiguration : IEntityTypeConfiguration<OksLogRequest>
{
    public void Configure(EntityTypeBuilder<OksLogRequest> builder)
    {
        builder.ToTable("OksLogRequest");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Message)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(x => x.Path).HasMaxLength(500);
        builder.Property(x => x.HttpMethod).HasMaxLength(10);
        builder.Property(x => x.ClientIp).HasMaxLength(50);
        builder.Property(x => x.UserId).HasMaxLength(100);
        builder.Property(x => x.CorrelationId).HasMaxLength(100);

        builder.Property(x => x.CreatedAtUtc).IsRequired();
    }
}