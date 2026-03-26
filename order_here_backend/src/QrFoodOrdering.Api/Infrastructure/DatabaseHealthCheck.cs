using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using QrFoodOrdering.Infrastructure.Persistence;
using System.Data;

namespace QrFoodOrdering.Api.Infrastructure;

public sealed class DatabaseHealthCheck : IHealthCheck
{
    private static readonly string[] RequiredTables =
    [
        "tables",
        "Orders",
        "qr_codes",
        "MenuItems",
        "AuditLogs",
        "IdempotencyRecords",
    ];

    private readonly QrFoodOrderingDbContext _db;

    public DatabaseHealthCheck(QrFoodOrderingDbContext db)
    {
        _db = db;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!await _db.Database.CanConnectAsync(cancellationToken))
                return HealthCheckResult.Unhealthy("Database is not reachable.");

            var pendingMigrations = await _db.Database.GetPendingMigrationsAsync(cancellationToken);
            if (pendingMigrations.Any())
            {
                return HealthCheckResult.Unhealthy(
                    "Database has pending migrations.",
                    data: new Dictionary<string, object>
                    {
                        ["pendingMigrations"] = pendingMigrations.ToArray()
                    }
                );
            }

            var missingTables = await GetMissingTablesAsync(cancellationToken);
            if (missingTables.Count > 0)
            {
                return HealthCheckResult.Unhealthy(
                    "Database schema is incomplete.",
                    data: new Dictionary<string, object>
                    {
                        ["missingTables"] = missingTables
                    }
                );
            }

            return HealthCheckResult.Healthy("Database is reachable and schema is ready.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Database connectivity check failed.", ex);
        }
    }

    private async Task<List<string>> GetMissingTablesAsync(CancellationToken cancellationToken)
    {
        await using var connection = _db.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT name FROM sqlite_master WHERE type = 'table';";

        var existingTables = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            if (!reader.IsDBNull(0))
                existingTables.Add(reader.GetString(0));
        }

        return RequiredTables.Where(x => !existingTables.Contains(x)).ToList();
    }
}
