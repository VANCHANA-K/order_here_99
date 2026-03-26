using QrFoodOrdering.Application.Abstractions;
using QrFoodOrdering.Application.Common.Audit;
using QrFoodOrdering.Application.Common.Errors;
using QrFoodOrdering.Application.Common.Exceptions;
using QrFoodOrdering.Domain.Orders;

namespace QrFoodOrdering.Application.Orders.CloseOrder;

public sealed class CloseOrderHandler
{
    private readonly IOrderRepository _repository;
    private readonly IUnitOfWork _uow;
    private readonly IAuditService _audit;

    public CloseOrderHandler(IOrderRepository repository, IUnitOfWork uow, IAuditService audit)
    {
        _repository = repository;
        _uow = uow;
        _audit = audit;
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
        await _audit.LogAsync(AuditEvents.OrderClosed, AuditEntities.Order, order.Id, null);
        await _uow.SaveChangesAsync(ct);
    }
}
