using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QrFoodOrdering.Domain.Qr;

namespace QrFoodOrdering.Infrastructure.Persistence.Configurations;

public sealed class QrCodeConfig : IEntityTypeConfiguration<QrCode>
{
    public void Configure(EntityTypeBuilder<QrCode> builder)
    {
        builder.ToTable("qr_codes");

        builder.HasKey(x => x.Id);

        builder
            .Property(x => x.Token)
            .HasMaxLength(128)
            .IsRequired();

        builder.HasIndex(x => x.Token).IsUnique();

        builder.Property(x => x.TableId).IsRequired();
        builder.Property(x => x.CreatedAtUtc).IsRequired();
        builder.Property(x => x.ExpiresAtUtc).IsRequired();
        builder.Property(x => x.IsActive).IsRequired();
    }
}
