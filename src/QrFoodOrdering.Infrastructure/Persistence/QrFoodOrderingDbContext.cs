using Microsoft.EntityFrameworkCore;
using QrFoodOrdering.Domain.Orders;

namespace QrFoodOrdering.Infrastructure.Persistence;

public sealed class QrFoodOrderingDbContext : DbContext
{
    public QrFoodOrderingDbContext(DbContextOptions<QrFoodOrderingDbContext> options)
        : base(options) { }

    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(QrFoodOrderingDbContext).Assembly);
    }
}
