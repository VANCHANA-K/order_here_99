using Microsoft.Extensions.Logging.Abstractions;
using QrFoodOrdering.Application.Abstractions;
using QrFoodOrdering.Application.Common.Idempotency;
using QrFoodOrdering.Application.Common.Observability;
using QrFoodOrdering.Application.Orders.CreateOrder;
using QrFoodOrdering.Application.Tables;
using QrFoodOrdering.Domain.Orders;
using QrFoodOrdering.Domain.Tables;
using QrFoodOrdering.Application.Common.Exceptions;
using QrFoodOrdering.Application.Common.Errors;

namespace QrFoodOrdering.UnitTests;

public class CreateOrderHandlerUnitTests
{
    private sealed class InMemoryOrderRepository : IOrderRepository
    {
        public readonly Dictionary<Guid, Order> Store = new();
        public int AddCalls { get; private set; }

        public Task AddAsync(Order order, CancellationToken ct)
        {
            AddCalls++;
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
            Store[order.Id] = order;
            return Task.CompletedTask;
        }
    }

    private sealed class InMemoryIdempotencyStore : IIdempotencyStore
    {
        private readonly Dictionary<string, Guid> _store = new();

        public Task<(bool Found, Guid OrderId)> TryGetAsync(string key, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(key))
                return Task.FromResult((false, Guid.Empty));

            var found = _store.TryGetValue(key, out var orderId);
            return Task.FromResult((found, orderId));
        }

        public Task MarkAsync(string key, Guid orderId, CancellationToken ct)
        {
            if (!string.IsNullOrWhiteSpace(key))
                _store[key] = orderId;

            return Task.CompletedTask;
        }
    }

    private sealed class InMemoryTablesRepository : ITablesRepository
    {
        public readonly Dictionary<Guid, Table> Store = new();

        public Task AddAsync(Table table, CancellationToken ct)
        {
            Store[table.Id] = table;
            return Task.CompletedTask;
        }

        public Task<Table?> GetByIdAsync(Guid id, CancellationToken ct)
        {
            Store.TryGetValue(id, out var table);
            return Task.FromResult(table);
        }

        public Task<List<Table>> GetAllAsync(CancellationToken ct) =>
            Task.FromResult(Store.Values.ToList());
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public int SaveChangesCalls { get; private set; }
        public int CommitCalls { get; private set; }

        public Task<IAsyncDisposable> BeginTransactionAsync(CancellationToken ct) =>
            Task.FromResult<IAsyncDisposable>(new NoopAsyncDisposable());

        public Task CommitAsync(CancellationToken ct)
        {
            CommitCalls++;
            return Task.CompletedTask;
        }

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

    private sealed class StubTraceContext : ITraceContext
    {
        public string TraceId { get; } = "trace-test";
    }

    [Fact]
    public async Task Create_order_should_save_once_and_commit()
    {
        var repo = new InMemoryOrderRepository();
        var tables = new InMemoryTablesRepository();
        var store = new InMemoryIdempotencyStore();
        var uow = new FakeUnitOfWork();
        var table = new Table("A01");
        await tables.AddAsync(table, CancellationToken.None);
        var handler = new CreateOrderHandler(
            repo,
            tables,
            store,
            uow,
            NullLogger<CreateOrderHandler>.Instance,
            new StubTraceContext()
        );

        var result = await handler.Handle(
            new CreateOrderCommand(table.Id, "abc"),
            CancellationToken.None
        );

        Assert.True(repo.Store.ContainsKey(result.OrderId));
        Assert.Equal(1, repo.AddCalls);
        Assert.Equal(1, uow.SaveChangesCalls);
        Assert.Equal(1, uow.CommitCalls);
    }

    [Fact]
    public async Task Create_order_should_scope_idempotency_key_by_use_case()
    {
        var repo = new InMemoryOrderRepository();
        var tables = new InMemoryTablesRepository();
        var store = new InMemoryIdempotencyStore();
        var uow = new FakeUnitOfWork();
        var existingOrderId = Guid.NewGuid();
        var table = new Table("A02");

        await tables.AddAsync(table, CancellationToken.None);

        await store.MarkAsync("abc", existingOrderId, CancellationToken.None);

        var handler = new CreateOrderHandler(
            repo,
            tables,
            store,
            uow,
            NullLogger<CreateOrderHandler>.Instance,
            new StubTraceContext()
        );

        var result = await handler.Handle(
            new CreateOrderCommand(table.Id, "abc"),
            CancellationToken.None
        );

        Assert.NotEqual(existingOrderId, result.OrderId);
        Assert.Equal(1, repo.AddCalls);
        Assert.Equal(1, uow.SaveChangesCalls);
    }

    [Fact]
    public async Task Create_order_with_unknown_table_should_throw_not_found()
    {
        var repo = new InMemoryOrderRepository();
        var tables = new InMemoryTablesRepository();
        var store = new InMemoryIdempotencyStore();
        var uow = new FakeUnitOfWork();
        var handler = new CreateOrderHandler(
            repo,
            tables,
            store,
            uow,
            NullLogger<CreateOrderHandler>.Instance,
            new StubTraceContext()
        );

        var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(new CreateOrderCommand(Guid.NewGuid(), "abc"), CancellationToken.None)
        );

        Assert.Equal(ApplicationErrorCodes.TableNotFound, ex.ErrorCode);
        Assert.Equal(0, repo.AddCalls);
    }

    [Fact]
    public async Task Create_order_with_inactive_table_should_throw_conflict()
    {
        var repo = new InMemoryOrderRepository();
        var tables = new InMemoryTablesRepository();
        var store = new InMemoryIdempotencyStore();
        var uow = new FakeUnitOfWork();
        var table = new Table("A03");
        table.Deactivate();
        await tables.AddAsync(table, CancellationToken.None);

        var handler = new CreateOrderHandler(
            repo,
            tables,
            store,
            uow,
            NullLogger<CreateOrderHandler>.Instance,
            new StubTraceContext()
        );

        var ex = await Assert.ThrowsAsync<ConflictException>(() =>
            handler.Handle(new CreateOrderCommand(table.Id, "abc"), CancellationToken.None)
        );

        Assert.Equal(ApplicationErrorCodes.TableInactive, ex.ErrorCode);
        Assert.Equal(0, repo.AddCalls);
    }
}
