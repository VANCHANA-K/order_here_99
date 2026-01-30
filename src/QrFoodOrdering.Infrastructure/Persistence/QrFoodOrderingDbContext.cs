using Microsoft.EntityFrameworkCore;
using QrFoodOrdering.Domain.Orders;

namespace QrFoodOrdering.Infrastructure.Persistence;

public sealed class QrFoodOrderingDbContext : DbContext
{
    public QrFoodOrderingDbContext(
        DbContextOptions<QrFoodOrderingDbContext> options)
        : base(options)
    {
    }

    // Aggregate Root
    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //
        // ============================
        // Order (Aggregate Root)
        // ============================
        //
        var order = modelBuilder.Entity<Order>();

        // Primary Key
        order.HasKey(o => o.Id);

        // Ignore computed / derived property
        // TotalAmount is calculated at runtime only
        order.Ignore(o => o.TotalAmount);

        // Aggregate children
        order
            .HasMany(o => o.Items)
            .WithOne()
            .HasForeignKey("OrderId")
            .OnDelete(DeleteBehavior.Cascade);

        // Use backing field for collection navigation so EF detects added items
        order
            .Navigation(o => o.Items)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        //
        // ============================
        // OrderItem (Entity)
        // ============================
        //
        var item = modelBuilder.Entity<OrderItem>();

        // Primary Key
        item.HasKey(i => i.Id);
        item.Property(i => i.Id)
            .ValueGeneratedNever();

        item.Property(i => i.ProductName)
            .IsRequired();

        item.Property(i => i.Quantity)
            .IsRequired();

        // Value Object: UnitPrice (Money)
        item.OwnsOne(i => i.UnitPrice, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("UnitPrice")
                .IsRequired();

            money.Property(m => m.Currency)
                .HasColumnName("Currency")
                .IsRequired();
        });
    }
}
