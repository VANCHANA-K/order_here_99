using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QrFoodOrdering.Application.Abstractions;
using QrFoodOrdering.Application.Common.Errors;
using QrFoodOrdering.Application.Common.Exceptions;
using QrFoodOrdering.Application.Common.Observability;
using QrFoodOrdering.Domain.Audit;
using QrFoodOrdering.Domain.Menu;
using QrFoodOrdering.Domain.Orders;
using QrFoodOrdering.Domain.Qr;
using QrFoodOrdering.Domain.Tables;
using QrFoodOrdering.Infrastructure.Persistence;
using QrFoodOrdering.IntegrationTests.Infrastructure;

namespace QrFoodOrdering.IntegrationTests;

public sealed class DatabaseExceptionTranslationTests
{
    [Fact]
    public async Task Unit_of_work_should_translate_duplicate_qr_token_to_conflict_exception()
    {
        await using var factory = new TestApiFactory();

        var ex = await Assert.ThrowsAsync<ConflictException>(() =>
            factory.ExecuteScopedAsync(async services =>
            {
                var db = services.GetRequiredService<QrFoodOrderingDbContext>();
                var uow = services.GetRequiredService<IUnitOfWork>();

                db.QrCodes.Add(new QrCode(Guid.NewGuid(), "dup-token", DateTime.UtcNow.AddDays(1)));
                await uow.SaveChangesAsync(CancellationToken.None);

                db.QrCodes.Add(new QrCode(Guid.NewGuid(), "dup-token", DateTime.UtcNow.AddDays(1)));
                await uow.SaveChangesAsync(CancellationToken.None);
            })
        );

        Assert.Equal(ApplicationErrorCodes.QrTokenAlreadyExists, ex.ErrorCode);
        Assert.Equal("QR token already exists.", ex.Message);
    }

    [Fact]
    public async Task Unit_of_work_should_translate_duplicate_menu_code_to_conflict_exception()
    {
        await using var factory = new TestApiFactory();

        var ex = await Assert.ThrowsAsync<ConflictException>(() =>
            factory.ExecuteScopedAsync(async services =>
            {
                var db = services.GetRequiredService<QrFoodOrderingDbContext>();
                var uow = services.GetRequiredService<IUnitOfWork>();

                db.MenuItems.Add(new MenuItem("M-DUP", "Pad Thai", 60m));
                await uow.SaveChangesAsync(CancellationToken.None);

                db.MenuItems.Add(new MenuItem("M-DUP", "Fried Rice", 55m));
                await uow.SaveChangesAsync(CancellationToken.None);
            })
        );

        Assert.Equal(ApplicationErrorCodes.MenuCodeAlreadyExists, ex.ErrorCode);
        Assert.Equal("Menu code already exists.", ex.Message);
    }

    [Fact]
    public async Task Unit_of_work_should_translate_order_concurrency_conflict_to_conflict_exception()
    {
        await using var factory = new TestApiFactory();

        var orderId = await factory.ExecuteDbContextAsync(async db =>
        {
            var order = new Order(Guid.NewGuid(), Guid.NewGuid());
            db.Orders.Add(order);
            await db.SaveChangesAsync();
            return order.Id;
        });

        using var firstScope = factory.Services.CreateScope();
        var firstDb = firstScope.ServiceProvider.GetRequiredService<QrFoodOrderingDbContext>();
        var firstUow = firstScope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var firstOrder = await firstDb.Orders.FirstAsync(x => x.Id == orderId);

        await factory.ExecuteDbContextAsync(async db =>
        {
            var order = await db.Orders.FirstAsync(x => x.Id == orderId);
            order.MarkPaid();
            await db.SaveChangesAsync();
        });

        firstOrder.Close();

        var ex = await Assert.ThrowsAsync<ConflictException>(() =>
            firstUow.SaveChangesAsync(CancellationToken.None)
        );

        Assert.Equal(ApplicationErrorCodes.ConcurrencyConflict, ex.ErrorCode);
        Assert.Equal("The resource was modified by another request. Please retry.", ex.Message);
    }

    [Fact]
    public async Task Unit_of_work_should_translate_table_concurrency_conflict_to_conflict_exception()
    {
        await using var factory = new TestApiFactory();

        var tableId = await factory.ExecuteDbContextAsync(async db =>
        {
            var table = new Table("TB-CONC");
            db.Tables.Add(table);
            await db.SaveChangesAsync();
            return table.Id;
        });

        using var firstScope = factory.Services.CreateScope();
        var firstDb = firstScope.ServiceProvider.GetRequiredService<QrFoodOrderingDbContext>();
        var firstUow = firstScope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var firstTable = await firstDb.Tables.FirstAsync(x => x.Id == tableId);

        await factory.ExecuteDbContextAsync(async db =>
        {
            var table = await db.Tables.FirstAsync(x => x.Id == tableId);
            table.Deactivate();
            await db.SaveChangesAsync();
        });

        firstTable.Deactivate();

        var ex = await Assert.ThrowsAsync<ConflictException>(() =>
            firstUow.SaveChangesAsync(CancellationToken.None)
        );

        Assert.Equal(ApplicationErrorCodes.ConcurrencyConflict, ex.ErrorCode);
        Assert.Equal("The resource was modified by another request. Please retry.", ex.Message);
    }

    [Fact]
    public async Task Audit_logs_should_reject_updates()
    {
        await using var factory = new TestApiFactory();

        var auditLogId = await factory.ExecuteDbContextAsync(async db =>
        {
            var log = new AuditLog("ORDER_CREATED", "Order", Guid.NewGuid(), "meta", "trace-immutable");
            db.AuditLogs.Add(log);
            await db.SaveChangesAsync();
            return log.Id;
        });

        var ex = await Assert.ThrowsAsync<ImmutableResourceException>(() =>
            factory.ExecuteDbContextAsync(async db =>
            {
                var log = await db.AuditLogs.FirstAsync(x => x.Id == auditLogId);
                db.Entry(log).Property(nameof(AuditLog.Metadata)).CurrentValue = "mutated";
                await db.SaveChangesAsync();
            })
        );

        Assert.Equal(ApplicationErrorCodes.AuditLogImmutable, ex.ErrorCode);
        Assert.Equal("AuditLogs are immutable and may only be appended.", ex.Message);
    }

    [Fact]
    public async Task Audit_logs_should_reject_deletes()
    {
        await using var factory = new TestApiFactory();

        var auditLogId = await factory.ExecuteDbContextAsync(async db =>
        {
            var log = new AuditLog("ORDER_CREATED", "Order", Guid.NewGuid(), "meta", "trace-immutable");
            db.AuditLogs.Add(log);
            await db.SaveChangesAsync();
            return log.Id;
        });

        var ex = await Assert.ThrowsAsync<ImmutableResourceException>(() =>
            factory.ExecuteDbContextAsync(async db =>
            {
                var log = await db.AuditLogs.FirstAsync(x => x.Id == auditLogId);
                db.AuditLogs.Remove(log);
                await db.SaveChangesAsync();
            })
        );

        Assert.Equal(ApplicationErrorCodes.AuditLogImmutable, ex.ErrorCode);
        Assert.Equal("AuditLogs are immutable and may only be appended.", ex.Message);
    }

    [Fact]
    public void Trace_id_policy_should_generate_background_trace_when_missing()
    {
        var traceId = TraceIdPolicy.Resolve("unknown", "job-sync");

        Assert.StartsWith("bg-job-sync-", traceId, StringComparison.Ordinal);
        Assert.NotEqual("unknown", traceId);
    }
}
