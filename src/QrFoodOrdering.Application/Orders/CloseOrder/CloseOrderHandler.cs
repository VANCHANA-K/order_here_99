using QrFoodOrdering.Application.Abstractions;

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

        order.Close(); // 🔥 rule อยู่ใน Domain

        await _repository.UpdateAsync(order, ct);
    }
}
