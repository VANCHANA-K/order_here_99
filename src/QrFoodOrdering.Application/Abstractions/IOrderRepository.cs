using QrFoodOrdering.Domain.Orders;

namespace QrFoodOrdering.Application.Abstractions;

public interface IOrderRepository
{
    Task AddAsync(Order order, CancellationToken ct);
    Task<Order?> GetByIdAsync(Guid orderId, CancellationToken ct);
    Task UpdateAsync(Order order, CancellationToken ct);
}

