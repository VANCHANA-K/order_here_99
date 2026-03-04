using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QrFoodOrdering.Domain.Audit;

namespace QrFoodOrdering.Infrastructure.Persistence.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.EventType).IsRequired().HasMaxLength(100);

        builder.Property(x => x.EntityType).IsRequired().HasMaxLength(100);

        builder.Property(x => x.Metadata).HasColumnType("TEXT");

        builder.Property(x => x.CreatedAtUtc).IsRequired();
    }
}
