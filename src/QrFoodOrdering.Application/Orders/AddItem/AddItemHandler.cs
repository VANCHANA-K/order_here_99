using Microsoft.Extensions.Logging;
using QrFoodOrdering.Application.Abstractions;
using QrFoodOrdering.Application.Common.Exceptions;
using QrFoodOrdering.Application.Common.Idempotency;
using QrFoodOrdering.Application.Common.Observability;
using QrFoodOrdering.Application.Common.Resilience;
using QrFoodOrdering.Domain.Orders;

namespace QrFoodOrdering.Application.Orders.AddItem;

public sealed class AddItemHandler
{
    private readonly IOrderRepository _repo;
    private readonly IIdempotencyStore _idempotency;
    private readonly IRetryPolicy _retry;
    private readonly ITraceContext _trace;
    private readonly ILogger<AddItemHandler> _logger;

    public AddItemHandler(
        IOrderRepository repo,
        IIdempotencyStore idempotency,
        IRetryPolicy retry,
        ITraceContext trace,
        ILogger<AddItemHandler> logger)
    {
        _repo = repo;
        _idempotency = idempotency;
        _retry = retry;
        _trace = trace;
        _logger = logger;
    }

    public async Task Handle(AddItemCommand command, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        // 1) Application-level validation
        if (string.IsNullOrWhiteSpace(command.ProductName))
            throw new InvalidRequestException("ProductName is required.");

        if (command.Quantity <= 0)
            throw new InvalidRequestException("Quantity must be positive.");

        if (command.UnitPrice <= 0)
            throw new InvalidRequestException("UnitPrice must be positive.");

        var traceId = _trace.TraceId;

        _logger.LogInformation("AddItemStarted {@data}", new
        {
            TraceId = traceId,
            OrderId = command.OrderId,
            Action = "ADD_ITEM"
        });

        // 2) Idempotency
        var hasKey = !string.IsNullOrWhiteSpace(command.IdempotencyKey);
        if (!hasKey)
        {
            await ExecuteOnce(command, ct);
            _logger.LogInformation("AddItemSucceeded {@data}", new
            {
                TraceId = traceId,
                OrderId = command.OrderId,
                Action = "ADD_ITEM",
                Status = "SUCCESS",
                Note = "No idempotency key provided"
            });
            return;
        }

        var key = $"orders:{command.OrderId}:add-item:{command.IdempotencyKey}";

        var existing = await _idempotency.TryGetAsync(key, ct);
        if (existing.Found)
        {
            _logger.LogInformation("AddItemIdempotentHit {@data}", new
            {
                TraceId = traceId,
                OrderId = command.OrderId,
                Action = "ADD_ITEM",
                IdempotencyKey = command.IdempotencyKey
            });
            return;
        }

        try
        {
            // 4) Safe retry only when idempotent
            await _retry.ExecuteAsync(async token =>
            {
                await ExecuteOnce(command, token);
            }, ct);

            // 5) Mark idempotent after success only
            await _idempotency.MarkAsync(key, command.OrderId, ct);

            _logger.LogInformation("AddItemSucceeded {@data}", new
            {
                TraceId = traceId,
                OrderId = command.OrderId,
                Action = "ADD_ITEM",
                Status = "SUCCESS"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AddItemFailed {@data}", new
            {
                TraceId = traceId,
                OrderId = command.OrderId,
                Action = "ADD_ITEM",
                Status = "FAILED"
            });
            throw;
        }
    }

    private async Task ExecuteOnce(AddItemCommand command, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var order = await _repo.GetByIdAsync(command.OrderId, ct)
            ?? throw new NotFoundException("Order not found");

        var item = new OrderItem(
            Guid.NewGuid(),
            command.ProductName.Trim(),
            command.Quantity,
            new Money(command.UnitPrice, "THB"));

        order.AddItem(item);

        await _repo.UpdateAsync(order, ct);
    }
}
