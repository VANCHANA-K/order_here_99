using QrFoodOrdering.Application.Abstractions;
using QrFoodOrdering.Domain.Orders;

namespace QrFoodOrdering.Application.Orders.AddItem;

public sealed class AddItemHandler
{
    private readonly IOrderRepository _orders;

    public AddItemHandler(IOrderRepository orders)
    {
   
        _orders = orders;
    }

    public async Task Handle(AddItemCommand command, CancellationToken ct)
    {

        if (string.IsNullOrWhiteSpace(command.ProductName))
            throw new ArgumentException("Product name is required");

        if (command.Quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero");

        if (command.UnitPrice <= 0)
            throw new ArgumentException("UnitPrice must be greater than zero");

        var order = await _orders.GetByIdAsync(command.OrderId, ct);
        if (order is null)
            throw new InvalidOperationException("Order not found");

        var item = new OrderItem(
            Guid.NewGuid(),
            command.ProductName.Trim(),
            command.Quantity,
            new Money(command.UnitPrice, "THB"));

        order.AddItem(item);

        await _orders.UpdateAsync(order, ct);
    }
}
