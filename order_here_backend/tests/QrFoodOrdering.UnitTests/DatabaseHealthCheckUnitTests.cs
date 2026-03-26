using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using QrFoodOrdering.Api.Infrastructure;
using QrFoodOrdering.Infrastructure.Persistence;

namespace QrFoodOrdering.UnitTests;

public sealed class DatabaseHealthCheckUnitTests
{
    [Fact]
    public async Task CheckHealthAsync_should_return_healthy_when_database_is_reachable_and_schema_is_ready()
    {
        var options = new DbContextOptionsBuilder<QrFoodOrderingDbContext>()
            .UseSqlite("Data Source=:memory:")
            .Options;

        await using var db = new QrFoodOrderingDbContext(options);
        await db.Database.OpenConnectionAsync();
        await db.Database.MigrateAsync();

        var check = new DatabaseHealthCheck(db);

        var result = await check.CheckHealthAsync(new HealthCheckContext(), CancellationToken.None);

        Assert.Equal(HealthStatus.Healthy, result.Status);
    }

    [Fact]
    public async Task CheckHealthAsync_should_return_unhealthy_when_migrations_are_pending()
    {
        var options = new DbContextOptionsBuilder<QrFoodOrderingDbContext>()
            .UseSqlite("Data Source=:memory:")
            .Options;

        await using var db = new QrFoodOrderingDbContext(options);
        await db.Database.OpenConnectionAsync();

        var check = new DatabaseHealthCheck(db);

        var result = await check.CheckHealthAsync(new HealthCheckContext(), CancellationToken.None);

        Assert.Equal(HealthStatus.Unhealthy, result.Status);
        Assert.Contains("pending migrations", result.Description, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CheckHealthAsync_should_return_unhealthy_when_database_is_not_reachable()
    {
        var options = new DbContextOptionsBuilder<QrFoodOrderingDbContext>()
            .UseSqlite("Data Source=/definitely/missing/path/healthcheck.db")
            .Options;

        await using var db = new QrFoodOrderingDbContext(options);
        var check = new DatabaseHealthCheck(db);

        var result = await check.CheckHealthAsync(new HealthCheckContext(), CancellationToken.None);

        Assert.Equal(HealthStatus.Unhealthy, result.Status);
    }
}
