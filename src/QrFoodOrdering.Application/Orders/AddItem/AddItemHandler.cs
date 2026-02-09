using Microsoft.Extensions.Logging;
using QrFoodOrdering.Application.Abstractions;
using QrFoodOrdering.Application.Common.Exceptions;
using QrFoodOrdering.Application.Common.Observability;
using QrFoodOrdering.Domain.Orders;

namespace QrFoodOrdering.Application.Orders.AddItem;

public sealed class AddItemHandler
{
    private readonly IOrderRepository _orders;
    private readonly ILogger<AddItemHandler> _logger;
    private readonly ITraceContext _trace;

    public AddItemHandler(
        IOrderRepository orders,
        ILogger<AddItemHandler> logger,
        ITraceContext trace
    )
    {
        _orders = orders;
        _logger = logger;
        _trace = trace;
    }

    public async Task Handle(AddItemCommand command, CancellationToken ct)
    {
        _logger.LogInformation(
            "AddItemStarted {@data}",
            new
            {
                TraceId = _trace.TraceId,
                Action = "ADD_ITEM",
                OrderId = command.OrderId,
            }
        );

        if (string.IsNullOrWhiteSpace(command.ProductName))
            throw new InvalidRequestException("Product name is required");

        if (command.Quantity <= 0)
            throw new InvalidRequestException("Quantity must be greater than zero");

        if (command.UnitPrice <= 0)
            throw new InvalidRequestException("UnitPrice must be greater than zero");

        try
        {
            var order = await _orders.GetByIdAsync(command.OrderId, ct);
            if (order is null)
                throw new NotFoundException("Order not found");

            var item = new OrderItem(
                Guid.NewGuid(),
                command.ProductName.Trim(),
                command.Quantity,
                new Money(command.UnitPrice, "THB")
            );

            order.AddItem(item);

            await _orders.UpdateAsync(order, ct);

            _logger.LogInformation(
                "AddItemSucceeded {@data}",
                new
                {
                    TraceId = _trace.TraceId,
                    Action = "ADD_ITEM",
                    OrderId = command.OrderId,
                    Status = "SUCCESS",
                }
            );
        }
        catch
        {
            _logger.LogWarning(
                "AddItemFailed {@data}",
                new
                {
                    TraceId = _trace.TraceId,
                    Action = "ADD_ITEM",
                    OrderId = command.OrderId,
                    Status = "FAILED",
                }
            );
            throw;
        }
    }
}
