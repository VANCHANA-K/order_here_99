using QrFoodOrdering.Application.Abstractions;
using QrFoodOrdering.Domain.Orders;

namespace QrFoodOrdering.Application.Orders.AddItem;

public sealed class AddItemHandler
{
    private readonly IOrderRepository _repository;

    public AddItemHandler(IOrderRepository repository)
    {
        _repository = repository;
    }

    public async Task Handle(AddItemCommand command, CancellationToken ct)
    {
        var order = await _repository.GetByIdAsync(command.OrderId, ct)
            ?? throw new InvalidOperationException("Order not found");

        var item = new OrderItem(
            Guid.NewGuid(),
            command.ProductName,
            command.Quantity,
            new Money(command.UnitPrice)
        );

        order.AddItem(item); // 🔥 rule อยู่ใน Domain

        await _repository.UpdateAsync(order, ct);
    }
}
