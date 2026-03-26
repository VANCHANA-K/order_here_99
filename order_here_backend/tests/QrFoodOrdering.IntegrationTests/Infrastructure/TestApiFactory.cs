using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using QrFoodOrdering.Domain.Menu;
using QrFoodOrdering.Domain.Audit;
using QrFoodOrdering.Infrastructure.Audit;
using QrFoodOrdering.Infrastructure.Persistence;

namespace QrFoodOrdering.IntegrationTests.Infrastructure;

public sealed class TestApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private SqliteConnection _connection = default!;
    private readonly InMemoryAuditLogWriter _auditLogWriter = new();

    public IReadOnlyList<AuditLog> WriterAuditLogs => _auditLogWriter.Logs;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<QrFoodOrderingDbContext>>();
            services.RemoveAll<QrFoodOrderingDbContext>();
            services.RemoveAll<IAuditLogWriter>();

            _connection = new SqliteConnection("Data Source=:memory:");
            _connection.Open();

            services.AddDbContext<QrFoodOrderingDbContext>(options =>
                options.UseSqlite(_connection)
            );
            services.AddSingleton<IAuditLogWriter>(_auditLogWriter);

            using var scope = services.BuildServiceProvider().CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<QrFoodOrderingDbContext>();
            db.Database.Migrate();
        });
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task<T> ExecuteDbContextAsync<T>(Func<QrFoodOrderingDbContext, Task<T>> action)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<QrFoodOrderingDbContext>();
        return await action(db);
    }

    public async Task ExecuteDbContextAsync(Func<QrFoodOrderingDbContext, Task> action)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<QrFoodOrderingDbContext>();
        await action(db);
    }

    public async Task<T> ExecuteScopedAsync<T>(Func<IServiceProvider, Task<T>> action)
    {
        using var scope = Services.CreateScope();
        return await action(scope.ServiceProvider);
    }

    public async Task ExecuteScopedAsync(Func<IServiceProvider, Task> action)
    {
        using var scope = Services.CreateScope();
        await action(scope.ServiceProvider);
    }

    public async Task<List<MenuItem>> SeedMenuItemsAsync(params (string Code, string Name, decimal Price)[] items)
    {
        return await ExecuteDbContextAsync(async db =>
        {
            var created = items.Select(x => new MenuItem(x.Code, x.Name, x.Price)).ToList();
            db.MenuItems.AddRange(created);
            await db.SaveChangesAsync();
            return created;
        });
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
        await _connection.DisposeAsync();
    }

    private sealed class InMemoryAuditLogWriter : IAuditLogWriter
    {
        private readonly List<AuditLog> _logs = [];

        public IReadOnlyList<AuditLog> Logs => _logs;

        public Task WriteAsync(AuditLog log)
        {
            _logs.Add(log);
            return Task.CompletedTask;
        }
    }
}
