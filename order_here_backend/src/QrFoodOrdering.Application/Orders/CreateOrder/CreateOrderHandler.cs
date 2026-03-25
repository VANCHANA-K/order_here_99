using Microsoft.Extensions.Logging;
using QrFoodOrdering.Application.Abstractions;
using QrFoodOrdering.Application.Common.Errors;
using QrFoodOrdering.Application.Common.Exceptions;
using QrFoodOrdering.Application.Common.Idempotency;
using QrFoodOrdering.Application.Common.Observability;
using QrFoodOrdering.Application.Tables;
using QrFoodOrdering.Domain.Orders;

namespace QrFoodOrdering.Application.Orders.CreateOrder;

public sealed class CreateOrderHandler
{
    private readonly IOrderRepository _repository;
    private readonly ITablesRepository _tablesRepository;
    private readonly IIdempotencyStore _idempotency;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<CreateOrderHandler> _logger;
    private readonly ITraceContext _trace;

    public CreateOrderHandler(
        IOrderRepository repository,
        ITablesRepository tablesRepository,
        IIdempotencyStore idempotency,
        IUnitOfWork uow,
        ILogger<CreateOrderHandler> logger,
        ITraceContext trace
    )
    {
        _repository = repository;
        _tablesRepository = tablesRepository;
        _idempotency = idempotency;
        _uow = uow;
        _logger = logger;
        _trace = trace;
    }

    public async Task<CreateOrderResult> Handle(CreateOrderCommand command, CancellationToken ct)
    {
        var key = string.IsNullOrWhiteSpace(command.IdempotencyKey)
            ? string.Empty
            : $"orders:create:{command.IdempotencyKey}";

        _logger.LogInformation(
            "CreateOrderStarted {@data}",
            new { TraceId = _trace.TraceId, Action = LogActions.CreateOrder }
        );

        try
        {
            // 1) Idempotency short-circuit
            var existing = await _idempotency.TryGetAsync(key, ct);
            if (existing.Found)
            {
                _logger.LogInformation(
                    "CreateOrderIdempotentHit {@data}",
                    new
                    {
                        TraceId = _trace.TraceId,
                        Action = LogActions.CreateOrder,
                        OrderId = existing.OrderId,
                        Status = LogStatuses.Hit,
                    }
                );
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

            var table =
                await _tablesRepository.GetByIdAsync(command.TableId, ct)
                ?? throw new NotFoundException(
                    ApplicationErrorCodes.TableNotFound,
                    "Table not found."
                );

            if (!table.IsActive)
                throw new ConflictException(
                    ApplicationErrorCodes.TableInactive,
                    "Table is inactive."
                );

            // 3) Create aggregate
            var order = new Order(Guid.NewGuid(), command.TableId);

            await _repository.AddAsync(order, ct);
            await _uow.SaveChangesAsync(ct);

            // 4) Mark idempotency after successful save
            await _idempotency.MarkAsync(key, order.Id, ct);

            await _uow.CommitAsync(ct);

            _logger.LogInformation(
                "CreateOrderSucceeded {@data}",
                new
                {
                    TraceId = _trace.TraceId,
                    Action = LogActions.CreateOrder,
                    OrderId = order.Id,
                    Status = LogStatuses.Success,
                }
            );

            return new CreateOrderResult(order.Id);
        }
        catch
        {
            _logger.LogWarning(
                "CreateOrderFailed {@data}",
                new
                {
                    TraceId = _trace.TraceId,
                    Action = LogActions.CreateOrder,
                    Status = LogStatuses.Failed,
                }
            );
            throw;
        }
    }
}
