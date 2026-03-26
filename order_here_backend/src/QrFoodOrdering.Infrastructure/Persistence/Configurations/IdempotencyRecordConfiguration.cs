using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QrFoodOrdering.Infrastructure.Idempotency;

namespace QrFoodOrdering.Infrastructure.Persistence.Configurations;

internal sealed class IdempotencyRecordConfiguration : IEntityTypeConfiguration<IdempotencyRecord>
{
    public void Configure(EntityTypeBuilder<IdempotencyRecord> builder)
    {
        builder.ToTable("IdempotencyRecords");

        builder.HasKey(x => x.Key);
        builder.Property(x => x.Key).IsRequired();
        builder.Property(x => x.RequestHash).IsRequired();
        builder.Property(x => x.OrderId).IsRequired();
        builder.Property(x => x.CreatedAtUtc).IsRequired();

        builder.HasIndex(x => x.Key).IsUnique();
    }
}
