namespace QrFoodOrdering.Domain.Orders;

public enum OrderStatus
{
    // Keep legacy persisted values stable:
    // Created(0) -> Pending(0), Paid(1) -> Confirmed(1), Cancelled(2), Closed(3) -> Completed(3)
    Pending = 0,
    Confirmed = 1,
    Cancelled = 2,
    Completed = 3,
    Cooking = 4,
    Ready = 5,
    Served = 6
}
