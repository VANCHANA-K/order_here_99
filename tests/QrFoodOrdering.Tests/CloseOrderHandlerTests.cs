using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using QrFoodOrdering.Application.Abstractions;
using QrFoodOrdering.Application.Orders.CloseOrder;
using QrFoodOrdering.Domain.Orders;
using Xunit;

namespace QrFoodOrdering.Tests;

public class CloseOrderHandlerTests
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

    [Fact]
    public async Task Close_order_when_open_should_set_closed_and_persist()
    {
        var repo = new InMemoryOrderRepository();
        var handler = new CloseOrderHandler(repo);

        var order = new Order(Guid.NewGuid());
        await repo.AddAsync(order, CancellationToken.None);

        await handler.Handle(order.Id, CancellationToken.None);

        Assert.Equal(OrderStatus.Closed, repo.Store[order.Id].Status);
        Assert.Equal(1, repo.UpdateCalls);
    }

    [Fact]
    public async Task Close_order_when_already_closed_should_be_noop_and_not_persist()
    {
        var repo = new InMemoryOrderRepository();
        var handler = new CloseOrderHandler(repo);

        var order = new Order(Guid.NewGuid());
        order.Close();
        await repo.AddAsync(order, CancellationToken.None);

        await handler.Handle(order.Id, CancellationToken.None);

        // still closed and no extra update
        Assert.Equal(OrderStatus.Closed, repo.Store[order.Id].Status);
        Assert.Equal(0, repo.UpdateCalls);
    }

    [Fact]
    public async Task Close_order_not_found_should_throw()
    {
        var repo = new InMemoryOrderRepository();
        var handler = new CloseOrderHandler(repo);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(Guid.NewGuid(), CancellationToken.None));
    }
}

