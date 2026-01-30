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

    public async Task AddAsync(Order order, CancellationToken ct)
    {
        _db.Orders.Add(order);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<Order?> GetByIdAsync(Guid orderId, CancellationToken ct)
    {
        return await _db.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == orderId, ct);
    }

    public async Task UpdateAsync(Order order, CancellationToken ct)
    {
        // With field-backed navigation configured, EF detects new items automatically
        await _db.SaveChangesAsync(ct);
    }
}
