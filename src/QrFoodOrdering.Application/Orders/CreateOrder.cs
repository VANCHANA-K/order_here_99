using QrFoodOrdering.Domain.Entities;

namespace QrFoodOrdering.Application.Orders;

public sealed class CreateOrder
{
    public Order Execute(int tableNumber)
    {
        // ใน Sprint 0 ยังไม่แตะ DB → แค่สร้าง domain object ให้ถูกต้อง
        return Order.CreateNew(tableNumber);
    }
}
