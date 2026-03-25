using QrFoodOrdering.Application.Abstractions;

namespace QrFoodOrdering.Application.Orders.GetOrder;

public sealed class GetOrderHandler
{
    private readonly IOrderRepository _repository;

    public GetOrderHandler(IOrderRepository repository)
    {
        _repository = repository;
    }

    public async Task<GetOrderResult?> Handle(GetOrderQuery query, CancellationToken ct)
    {
        var order = await _repository.GetByIdAsync(query.OrderId, ct);
        if (order is null) return null;

        return new GetOrderResult(
            order.Id,
            order.Status,
            order.TotalAmount.Amount
        );
    }
}
