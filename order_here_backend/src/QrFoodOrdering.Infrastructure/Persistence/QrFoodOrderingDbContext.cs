using Microsoft.EntityFrameworkCore;
using QrFoodOrdering.Application.Common.Errors;
using QrFoodOrdering.Application.Common.Exceptions;
using QrFoodOrdering.Application.Common.Validation;
using QrFoodOrdering.Domain.Audit;
using QrFoodOrdering.Domain.Menu;
using QrFoodOrdering.Domain.Orders;
using QrFoodOrdering.Domain.Qr;
using QrFoodOrdering.Domain.Tables;
using QrFoodOrdering.Infrastructure.Idempotency;
using QrFoodOrdering.Infrastructure.Persistence.Configurations;

namespace QrFoodOrdering.Infrastructure.Persistence;

public sealed class QrFoodOrderingDbContext : DbContext
{
    public QrFoodOrderingDbContext(DbContextOptions<QrFoodOrderingDbContext> options)
        : base(options) { }

    // Aggregate Root
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Table> Tables => Set<Table>();
    public DbSet<QrCode> QrCodes => Set<QrCode>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<MenuItem> MenuItems => Set<MenuItem>();
    public DbSet<IdempotencyRecord> IdempotencyRecords => Set<IdempotencyRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply configuration for Table aggregate
        modelBuilder.ApplyConfiguration(new TableConfig());
        modelBuilder.ApplyConfiguration(new QrCodeConfig());
        modelBuilder.ApplyConfiguration(new MenuItemConfig());
        modelBuilder.ApplyConfiguration(new IdempotencyRecordConfiguration());

        //
        // ============================
        // Order (Aggregate Root)
        // ============================
        //
        var order = modelBuilder.Entity<Order>();

        // Primary Key
        order.HasKey(o => o.Id);

        // Persist creation timestamp
        order.Property(o => o.CreatedAtUtc).IsRequired();
        order.Property(o => o.TableId).IsRequired();
        order.Property(o => o.RowVersion).IsConcurrencyToken().HasDefaultValue(1L);
        order.HasIndex(o => o.TableId);

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
        order.Navigation(o => o.Items).UsePropertyAccessMode(PropertyAccessMode.Field);

        //
        // ============================
        // OrderItem (Entity)
        // ============================
        //
        var item = modelBuilder.Entity<OrderItem>();

        // Primary Key
        item.HasKey(i => i.Id);
        item.Property(i => i.Id).ValueGeneratedNever();

        item.Property(i => i.ProductName).IsRequired();

        item.Property(i => i.Quantity).IsRequired();

        // Value Object: UnitPrice (Money)
        item.OwnsOne(
            i => i.UnitPrice,
            money =>
            {
                money.Property(m => m.Amount).HasColumnName("UnitPrice").IsRequired();

                money.Property(m => m.Currency).HasColumnName("Currency").IsRequired();
            }
        );
    }

    public override int SaveChanges()
    {
        EnforceAuditLogImmutability();
        PrepareConcurrencyTokens();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        EnforceAuditLogImmutability();
        PrepareConcurrencyTokens();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void EnforceAuditLogImmutability()
    {
        foreach (var entry in ChangeTracker.Entries<AuditLog>())
        {
            if (entry.State is EntityState.Modified or EntityState.Deleted)
            {
                throw new ImmutableResourceException(
                    ApplicationErrorCodes.AuditLogImmutable,
                    RequestValidationMessages.AuditLogImmutable
                );
            }
        }
    }

    private void PrepareConcurrencyTokens()
    {
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Metadata.FindProperty("RowVersion") is null)
                continue;

            var rowVersion = entry.Property("RowVersion");

            if (entry.State == EntityState.Added)
            {
                var currentValue = rowVersion.CurrentValue is long value ? value : 0L;
                if (currentValue <= 0)
                    rowVersion.CurrentValue = 1L;
            }
            else if (entry.State == EntityState.Modified)
            {
                var originalValue = rowVersion.OriginalValue is long value ? value : 0L;
                rowVersion.CurrentValue = originalValue + 1L;
            }
        }
    }
}
