using QrFoodOrdering.Application.Abstractions;
using QrFoodOrdering.Application.Common.Errors;
using QrFoodOrdering.Application.Common.Exceptions;
using QrFoodOrdering.Domain.Orders;

namespace QrFoodOrdering.Application.Orders.CloseOrder;

public sealed class CloseOrderHandler
{
    private readonly IOrderRepository _repository;
    private readonly IUnitOfWork _uow;

    public CloseOrderHandler(IOrderRepository repository, IUnitOfWork uow)
    {
        _repository = repository;
        _uow = uow;
    }

    public async Task Handle(CloseOrderCommand command, CancellationToken ct)
    {
        var order = await _repository.GetByIdAsync(command.OrderId, ct)
            ?? throw new NotFoundException(
                ApplicationErrorCodes.OrderNotFound,
                "Order not found"
            );

        // Double submit safe: if already closed, treat as no-op
        if (order.Status == OrderStatus.Completed)
            return;

        order.Close(); // 🔥 rule อยู่ใน Domain

        await _repository.UpdateAsync(order, ct);
        await _uow.SaveChangesAsync(ct);
    }
}
