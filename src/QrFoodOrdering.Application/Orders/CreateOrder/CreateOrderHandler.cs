using QrFoodOrdering.Application.Abstractions;
using QrFoodOrdering.Domain.Orders;

namespace QrFoodOrdering.Application.Orders.CreateOrder;

public sealed class CreateOrderHandler
{
    private readonly IOrderRepository _repository;

    public CreateOrderHandler(IOrderRepository repository)
    {
        _repository = repository;
    }

    public async Task<CreateOrderResult> Handle(
        CreateOrderCommand command,
        CancellationToken ct)
    {
        var order = new Order(Guid.NewGuid());

        await _repository.AddAsync(order, ct);

        return new CreateOrderResult(order.Id);
    }
}
