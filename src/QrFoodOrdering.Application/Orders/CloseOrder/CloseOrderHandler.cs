using QrFoodOrdering.Application.Abstractions;
using QrFoodOrdering.Domain.Orders;

namespace QrFoodOrdering.Application.Orders.CloseOrder;

public sealed class CloseOrderHandler
{
    private readonly IOrderRepository _repository;

    public CloseOrderHandler(IOrderRepository repository)
    {
        _repository = repository;
    }

    public async Task Handle(Guid orderId, CancellationToken ct)
    {
        var order = await _repository.GetByIdAsync(orderId, ct)
            ?? throw new InvalidOperationException("Order not found");

        // Double submit safe: if already closed, treat as no-op
        if (order.Status == OrderStatus.Closed)
            return;

        order.Close(); // 🔥 rule อยู่ใน Domain

        await _repository.UpdateAsync(order, ct);
    }
}
