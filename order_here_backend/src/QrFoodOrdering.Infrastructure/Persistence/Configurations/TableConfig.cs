using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QrFoodOrdering.Domain.Tables;

namespace QrFoodOrdering.Infrastructure.Persistence.Configurations;

public sealed class TableConfig : IEntityTypeConfiguration<Table>
{
    public void Configure(EntityTypeBuilder<Table> b)
    {
        b.ToTable("tables");
        b.HasKey(x => x.Id);

        b.Property(x => x.Code).IsRequired().HasMaxLength(20);

        b.HasIndex(x => x.Code).IsUnique();

        b.Property(x => x.IsActive).IsRequired();
        b.Property(x => x.RowVersion).IsConcurrencyToken().HasDefaultValue(1L);
    }
}
