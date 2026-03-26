using Microsoft.Extensions.Logging;
using QrFoodOrdering.Application.Abstractions;
using QrFoodOrdering.Application.Common.Audit;
using QrFoodOrdering.Application.Common.Errors;
using QrFoodOrdering.Application.Common.Exceptions;
using QrFoodOrdering.Application.Common.Idempotency;
using QrFoodOrdering.Application.Common.Observability;
using QrFoodOrdering.Application.Common.Resilience;
using QrFoodOrdering.Application.Common.Validation;
using QrFoodOrdering.Domain.Orders;

namespace QrFoodOrdering.Application.Orders.AddItem;

public sealed class AddItemHandler
{
    private readonly IOrderRepository _repo;
    private readonly IIdempotencyStore _idempotency;
    private readonly IRetryPolicy _retry;
    private readonly IUnitOfWork _uow;
    private readonly IAuditService _audit;
    private readonly ITraceContext _trace;
    private readonly ILogger<AddItemHandler> _logger;

    public AddItemHandler(
        IOrderRepository repo,
        IIdempotencyStore idempotency,
        IRetryPolicy retry,
        IUnitOfWork uow,
        IAuditService audit,
        ITraceContext trace,
        ILogger<AddItemHandler> logger
    )
    {
        _repo = repo;
        _idempotency = idempotency;
        _retry = retry;
        _uow = uow;
        _audit = audit;
        _trace = trace;
        _logger = logger;
    }

    public async Task Handle(AddItemCommand command, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        // 1) Application-level validation
        if (string.IsNullOrWhiteSpace(command.ProductName))
            throw new InvalidRequestException(
                ApplicationErrorCodes.ProductNameRequired,
                RequestValidationMessages.ProductNameRequired
            );

        if (command.Quantity <= 0)
            throw new InvalidRequestException(
                ApplicationErrorCodes.InvalidQuantity,
                RequestValidationMessages.QuantityMustBeGreaterThanZero
            );

        if (command.UnitPrice <= 0)
            throw new InvalidRequestException(
                ApplicationErrorCodes.UnitPriceInvalid,
                RequestValidationMessages.UnitPriceMustBePositive
            );

        var traceId = _trace.TraceId;

        _logger.LogInformation(
            "AddItemStarted {@data}",
            new
            {
                TraceId = traceId,
                OrderId = command.OrderId,
                Action = LogActions.AddItem,
            }
        );

        // 2) Idempotency
        var hasKey = !string.IsNullOrWhiteSpace(command.IdempotencyKey);
        if (!hasKey)
        {
            await ExecuteOnce(command, ct);
            await _uow.SaveChangesAsync(ct);
            _logger.LogInformation(
                "AddItemSucceeded {@data}",
                new
                {
                    TraceId = traceId,
                    OrderId = command.OrderId,
                    Action = LogActions.AddItem,
                    Status = LogStatuses.Success,
                    Note = "No idempotency key provided",
                }
            );
            return;
        }

        var key = $"orders:{command.OrderId}:add-item:{command.IdempotencyKey}";
        var requestHash = IdempotencyRequestHasher.Compute(
            FormattableString.Invariant(
                $"{command.OrderId:D}|{command.ProductName.Trim()}|{command.Quantity}|{command.UnitPrice}"
            )
        );

        var existing = await _idempotency.TryGetAsync(key, ct);
        if (existing.Found)
        {
            EnsureMatchingRequestHash(existing, requestHash);
            _logger.LogInformation(
                "AddItemIdempotentHit {@data}",
                new
                {
                    TraceId = traceId,
                    OrderId = command.OrderId,
                    Action = LogActions.AddItem,
                    IdempotencyKey = command.IdempotencyKey,
                }
            );
            return;
        }

        try
        {
            // 4) Safe retry only when idempotent
            await _retry.ExecuteAsync(
                async token =>
                {
                    await ExecuteOnce(command, token);
                    await _idempotency.MarkAsync(key, requestHash, command.OrderId, token);
                    await _uow.SaveChangesAsync(token);
                },
                ct
            );

            _logger.LogInformation(
                "AddItemSucceeded {@data}",
                new
                {
                    TraceId = traceId,
                    OrderId = command.OrderId,
                    Action = LogActions.AddItem,
                    Status = LogStatuses.Success,
                }
            );
        }
        catch (ConflictException ex) when (ex.ErrorCode == ApplicationErrorCodes.IdempotencyKeyConflict)
        {
            var persisted = await _idempotency.TryGetAsync(key, ct);
            if (persisted.Found)
            {
                EnsureMatchingRequestHash(persisted, requestHash);
                return;
            }

            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "AddItemFailed {@data}",
                new
                {
                    TraceId = traceId,
                    OrderId = command.OrderId,
                    Action = LogActions.AddItem,
                    Status = LogStatuses.Failed,
                }
            );
            throw;
        }
    }

    private async Task ExecuteOnce(AddItemCommand command, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var order =
            await _repo.GetByIdAsync(command.OrderId, ct)
            ?? throw new NotFoundException(
                ApplicationErrorCodes.OrderNotFound,
                "Order not found"
            );

        var item = new OrderItem(
            Guid.NewGuid(),
            command.ProductName.Trim(),
            command.Quantity,
            new Money(command.UnitPrice, "THB")
        );

        order.AddItem(item);

        await _repo.UpdateAsync(order, ct);
        await _audit.LogAsync(
            AuditEvents.OrderItemAdded,
            AuditEntities.Order,
            order.Id,
            $"productName={command.ProductName.Trim()};quantity={command.Quantity};traceId={_trace.TraceId}"
        );
    }

    private static void EnsureMatchingRequestHash(IdempotencyResult existing, string requestHash)
    {
        if (!string.Equals(existing.RequestHash, requestHash, StringComparison.Ordinal))
            throw new ConflictException(
                ApplicationErrorCodes.IdempotencyKeyPayloadMismatch,
                RequestValidationMessages.IdempotencyKeyPayloadMismatch
            );
    }
}
