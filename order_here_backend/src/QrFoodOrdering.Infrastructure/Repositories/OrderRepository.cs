using Microsoft.EntityFrameworkCore;
using QrFoodOrdering.Application.Abstractions;
using QrFoodOrdering.Domain.Orders;
using QrFoodOrdering.Infrastructure.Persistence;

namespace QrFoodOrdering.Infrastructure.Repositories;

public sealed class OrderRepository : IOrderRepository
{
    private readonly QrFoodOrderingDbContext _db;

    public OrderRepository(QrFoodOrderingDbContext db)
    {
        _db = db;
    }

    public Task AddAsync(Order order, CancellationToken ct)
    {
        _db.Orders.Add(order);
        return Task.CompletedTask;
    }

    public async Task<Order?> GetByIdAsync(Guid orderId, CancellationToken ct)
    {
        return await _db.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == orderId, ct);
    }

    public Task UpdateAsync(Order order, CancellationToken ct)
    {
        // Entities loaded via GetByIdAsync are already tracked by EF Core.
        return Task.CompletedTask;
    }
}
