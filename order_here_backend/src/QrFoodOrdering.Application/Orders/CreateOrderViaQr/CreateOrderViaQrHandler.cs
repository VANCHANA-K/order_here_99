using System.Text.Json;
using System.Collections.Concurrent;
using QrFoodOrdering.Application.Abstractions;
using QrFoodOrdering.Application.Common.Audit;
using QrFoodOrdering.Application.Common.Exceptions;
using QrFoodOrdering.Application.Common.Idempotency;
using QrFoodOrdering.Application.Common.Observability;
using QrFoodOrdering.Application.Tables;
using QrFoodOrdering.Domain.Orders;

namespace QrFoodOrdering.Application.Orders.CreateOrderViaQr;

public sealed class CreateOrderViaQrHandler
{
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> IdempotencyLocks = new();

    private readonly IUnitOfWork _uow;
    private readonly ITablesRepository _tables;
    private readonly IMenuRepository _menu;
    private readonly IOrderRepository _orders;
    private readonly IAuditService _audit;
    private readonly IIdempotencyStore _idempotency;
    private readonly ITraceContext _trace;

    public CreateOrderViaQrHandler(
        IUnitOfWork uow,
        ITablesRepository tables,
        IMenuRepository menu,
        IOrderRepository orders,
        IAuditService audit,
        IIdempotencyStore idempotency,
        ITraceContext trace
    )
    {
        _uow = uow;
        _tables = tables;
        _menu = menu;
        _orders = orders;
        _audit = audit;
        _idempotency = idempotency;
        _trace = trace;
    }

    public async Task<CreateOrderViaQrResult> Handle(
        CreateOrderViaQrCommand cmd,
        CancellationToken ct
    )
    {
        // 1) Validate table
        var table = await _tables.GetByIdAsync(cmd.TableId, ct);
        if (table is null)
            throw new InvalidRequestException("TABLE_NOT_FOUND", "Table not found.");

        if (!table.IsActive)
            throw new ConflictException("TABLE_INACTIVE", "Table is inactive.");

        // 2) Validate items
        if (cmd.Items is null || cmd.Items.Count == 0)
            throw new InvalidRequestException("EMPTY_ITEMS", "At least one item is required.");

        foreach (var it in cmd.Items)
        {
            if (it.Quantity <= 0)
                throw new InvalidRequestException(
                    "INVALID_QTY",
                    "Quantity must be greater than 0."
                );
        }

        var key = cmd.IdempotencyKey;
        if (string.IsNullOrWhiteSpace(key))
        {
            return await CreateOrderAsync(cmd, key, ct);
        }

        // Keep idempotency read/create/mark in one critical section per key.
        var gate = IdempotencyLocks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
        await gate.WaitAsync(ct);
        try
        {
            return await CreateOrderAsync(cmd, key, ct);
        }
        finally
        {
            gate.Release();
        }
    }

    private async Task<CreateOrderViaQrResult> CreateOrderAsync(
        CreateOrderViaQrCommand cmd,
        string? key,
        CancellationToken ct
    )
    {
        // 3) Optional idempotency short-circuit
        if (!string.IsNullOrWhiteSpace(key))
        {
            var existing = await _idempotency.TryGetAsync(key, ct);
            if (existing.Found)
            {
                var existingOrder = await _orders.GetByIdAsync(existing.OrderId, ct);
                if (existingOrder is not null)
                {
                    return new CreateOrderViaQrResult(
                        existingOrder.Id,
                        existingOrder.Status,
                        existingOrder.CreatedAtUtc
                    );
                }

                return new CreateOrderViaQrResult(
                    existing.OrderId,
                    OrderStatus.Pending,
                    DateTime.UtcNow
                );
            }
        }

        // 4) Load menu items, validate active/available
        var menuItems = await _menu.GetByIdsAsync(cmd.Items.Select(x => x.MenuItemId).ToList(), ct);

        if (menuItems.Count != cmd.Items.Count)
            throw new InvalidRequestException("ITEM_NOT_FOUND", "One or more items do not exist.");

        foreach (var mi in menuItems)
        {
            if (!mi.IsActive)
                throw new InvalidRequestException(
                    "ITEM_INACTIVE",
                    "One or more items are inactive."
                );

            if (!mi.IsAvailable)
                throw new InvalidRequestException(
                    "ITEM_UNAVAILABLE",
                    "One or more items are unavailable."
                );
        }

        // 5) Create Order
        var order = new Order(Guid.NewGuid(), cmd.TableId, OrderStatus.Pending);

        foreach (var req in cmd.Items)
        {
            var mi = menuItems.Single(x => x.Id == req.MenuItemId);
            var price = new Money(mi.Price, "THB");
            var item = new OrderItem(Guid.NewGuid(), mi.Name, req.Quantity, price);
            order.AddItem(item);
        }

        await _orders.AddAsync(order, ct);

        if (!string.IsNullOrWhiteSpace(key))
            await _idempotency.MarkAsync(key, order.Id, ct);

        var auditMetadata = JsonSerializer.Serialize(
            new
            {
                OrderId = order.Id,
                TableId = cmd.TableId,
                Items = cmd.Items.Select(i => new { i.MenuItemId, i.Quantity }).ToList(),
                TraceId = _trace.TraceId,
                TimestampUtc = DateTime.UtcNow,
            }
        );

        await _audit.LogAsync("OrderPlacedViaQR", "Order", order.Id, auditMetadata);

        await _uow.SaveChangesAsync(ct);

        return new CreateOrderViaQrResult(order.Id, order.Status, order.CreatedAtUtc);
    }
}
