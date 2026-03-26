using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace QrFoodOrdering.Infrastructure.Persistence;

public sealed class QrFoodOrderingDesignTimeDbContextFactory
    : IDesignTimeDbContextFactory<QrFoodOrderingDbContext>
{
    public QrFoodOrderingDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<QrFoodOrderingDbContext>();
        optionsBuilder.UseSqlite("Data Source=qrfood.dev.db");
        return new QrFoodOrderingDbContext(optionsBuilder.Options);
    }
}
