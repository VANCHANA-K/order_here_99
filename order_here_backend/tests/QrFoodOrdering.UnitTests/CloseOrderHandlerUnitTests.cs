using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using QrFoodOrdering.Application.Abstractions;
using QrFoodOrdering.Application.Common.Audit;
using QrFoodOrdering.Application.Common.Errors;
using QrFoodOrdering.Application.Common.Exceptions;
using QrFoodOrdering.Application.Orders.CloseOrder;
using QrFoodOrdering.Domain.Orders;
using Xunit;

namespace QrFoodOrdering.UnitTests;

public class CloseOrderHandlerUnitTests
{
    private sealed class InMemoryOrderRepository : IOrderRepository
    {
        public readonly Dictionary<Guid, Order> Store = new();
        public int UpdateCalls { get; private set; }

        public Task AddAsync(Order order, CancellationToken ct)
        {
            Store[order.Id] = order;
            return Task.CompletedTask;
        }

        public Task<Order?> GetByIdAsync(Guid orderId, CancellationToken ct)
        {
            Store.TryGetValue(orderId, out var order);
            return Task.FromResult(order);
        }

        public Task UpdateAsync(Order order, CancellationToken ct)
        {
            UpdateCalls++;
            // Order state already mutated by domain method
            Store[order.Id] = order;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public int SaveChangesCalls { get; private set; }

        public Task<IAsyncDisposable> BeginTransactionAsync(CancellationToken ct) =>
            Task.FromResult<IAsyncDisposable>(new NoopAsyncDisposable());

        public Task CommitAsync(CancellationToken ct) => Task.CompletedTask;

        public Task SaveChangesAsync(CancellationToken ct)
        {
            SaveChangesCalls++;
            return Task.CompletedTask;
        }
    }

    private sealed class NoopAsyncDisposable : IAsyncDisposable
    {
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }

    private sealed class FakeAuditService : IAuditService
    {
        public readonly List<(string EventType, string EntityType, Guid EntityId)> Entries = new();

        public Task LogAsync(string eventType, string entityType, Guid entityId, string? metadata = null)
        {
            Entries.Add((eventType, entityType, entityId));
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task Close_order_when_open_should_set_completed_and_persist()
    {
        var repo = new InMemoryOrderRepository();
        var uow = new FakeUnitOfWork();
        var audit = new FakeAuditService();
        var handler = new CloseOrderHandler(repo, uow, audit);

        var order = new Order(Guid.NewGuid(), Guid.NewGuid());
        await repo.AddAsync(order, CancellationToken.None);

        await handler.Handle(new CloseOrderCommand(order.Id), CancellationToken.None);

        Assert.Equal(OrderStatus.Completed, repo.Store[order.Id].Status);
        Assert.Equal(1, repo.UpdateCalls);
        Assert.Equal(1, uow.SaveChangesCalls);
        Assert.Contains(audit.Entries, x => x.EventType == AuditEvents.OrderClosed && x.EntityId == order.Id);
    }

    [Fact]
    public async Task Close_order_when_already_completed_should_be_noop_and_not_persist()
    {
        var repo = new InMemoryOrderRepository();
        var uow = new FakeUnitOfWork();
        var audit = new FakeAuditService();
        var handler = new CloseOrderHandler(repo, uow, audit);

        var order = new Order(Guid.NewGuid(), Guid.NewGuid());
        order.Close();
        await repo.AddAsync(order, CancellationToken.None);

        await handler.Handle(new CloseOrderCommand(order.Id), CancellationToken.None);

        // still completed and no extra update
        Assert.Equal(OrderStatus.Completed, repo.Store[order.Id].Status);
        Assert.Equal(0, repo.UpdateCalls);
        Assert.Equal(0, uow.SaveChangesCalls);
        Assert.Empty(audit.Entries);
    }

    [Fact]
    public async Task Close_order_not_found_should_throw_not_found()
    {
        var repo = new InMemoryOrderRepository();
        var uow = new FakeUnitOfWork();
        var audit = new FakeAuditService();
        var handler = new CloseOrderHandler(repo, uow, audit);

        var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(new CloseOrderCommand(Guid.NewGuid()), CancellationToken.None));

        Assert.Equal(ApplicationErrorCodes.OrderNotFound, ex.ErrorCode);
    }
}
