using Microsoft.Extensions.Logging;
using QrFoodOrdering.Application.Abstractions;
using QrFoodOrdering.Application.Common.Idempotency;
using QrFoodOrdering.Domain.Orders;

namespace QrFoodOrdering.Application.Orders.CreateOrder;

public sealed class CreateOrderHandler
{
    private readonly IOrderRepository _repository;
    private readonly IIdempotencyStore _idempotency;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<CreateOrderHandler> _logger;

    public CreateOrderHandler(
        IOrderRepository repository,
        IIdempotencyStore idempotency,
        IUnitOfWork uow,
        ILogger<CreateOrderHandler> logger)
    {
        _repository = repository;
        _idempotency = idempotency;
        _uow = uow;
        _logger = logger;
    }

    public async Task<CreateOrderResult> Handle(
        CreateOrderCommand command,
        CancellationToken ct)
    {
        var key = command.IdempotencyKey ?? string.Empty;

        // 1) Idempotency short-circuit
        var existing = await _idempotency.TryGetAsync(key, ct);
        if (existing.Found)
        {
            _logger.LogInformation("CreateOrder idempotent hit: {Key} -> {OrderId}", key, existing.OrderId);
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

        _logger.LogInformation("CreateOrder succeeded: {OrderId}", order.Id);

        return new CreateOrderResult(order.Id);
    }
}
