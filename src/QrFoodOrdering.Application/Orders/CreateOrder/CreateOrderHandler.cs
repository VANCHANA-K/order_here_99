using Microsoft.Extensions.Logging;
using QrFoodOrdering.Application.Abstractions;
using QrFoodOrdering.Application.Common.Idempotency;
using QrFoodOrdering.Application.Common.Observability;
using QrFoodOrdering.Domain.Orders;

namespace QrFoodOrdering.Application.Orders.CreateOrder;

public sealed class CreateOrderHandler
{
    private readonly IOrderRepository _repository;
    private readonly IIdempotencyStore _idempotency;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<CreateOrderHandler> _logger;
    private readonly ITraceContext _trace;

    public CreateOrderHandler(
        IOrderRepository repository,
        IIdempotencyStore idempotency,
        IUnitOfWork uow,
        ILogger<CreateOrderHandler> logger,
        ITraceContext trace)
    {
        _repository = repository;
        _idempotency = idempotency;
        _uow = uow;
        _logger = logger;
        _trace = trace;
    }

    public async Task<CreateOrderResult> Handle(
        CreateOrderCommand command,
        CancellationToken ct)
    {
        var key = command.IdempotencyKey ?? string.Empty;

        _logger.LogInformation("CreateOrderStarted {@data}", new
        {
            TraceId = _trace.TraceId,
            Action = "CREATE_ORDER"
        });

        try
        {
            // 1) Idempotency short-circuit
            var existing = await _idempotency.TryGetAsync(key, ct);
            if (existing.Found)
            {
                _logger.LogInformation("CreateOrderIdempotentHit {@data}", new
                {
                    TraceId = _trace.TraceId,
                    Action = "CREATE_ORDER",
                    OrderId = existing.OrderId,
                    Status = "HIT"
                });
                return new CreateOrderResult(existing.OrderId);
            }

            await using var tx = await _uow.BeginTransactionAsync(ct);

            // 2) Re-check within transaction
            existing = await _idempotency.TryGetAsync(key, ct);
            if (existing.Found)
            {
                await _uow.CommitAsync(ct);
                return new CreateOrderResult(existing.OrderId);
            }

            // 3) Create aggregate (request currently empty placeholder)
            var order = new Order(Guid.NewGuid());

            await _repository.AddAsync(order, ct);

            // 4) Mark idempotency after successful save
            await _idempotency.MarkAsync(key, order.Id, ct);

            await _uow.CommitAsync(ct);

            _logger.LogInformation("CreateOrderSucceeded {@data}", new
            {
                TraceId = _trace.TraceId,
                Action = "CREATE_ORDER",
                OrderId = order.Id,
                Status = "SUCCESS"
            });

            return new CreateOrderResult(order.Id);
        }
        catch
        {
            _logger.LogWarning("CreateOrderFailed {@data}", new
            {
                TraceId = _trace.TraceId,
                Action = "CREATE_ORDER",
                Status = "FAILED"
            });
            throw;
        }
    }
}
