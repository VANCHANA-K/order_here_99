namespace QrFoodOrdering.Domain.Entities;

public sealed class Order
{
    public Guid Id { get; }
    public int TableNumber { get; }
    public DateTimeOffset CreatedAt { get; }
    public OrderStatus Status { get; private set; }

    private Order(Guid id, int tableNumber, DateTimeOffset createdAt, OrderStatus status)
    {
        if (tableNumber <= 0) throw new ArgumentOutOfRangeException(nameof(tableNumber));
        Id = id;
        TableNumber = tableNumber;
        CreatedAt = createdAt;
        Status = status;
    }

    public static Order CreateNew(int tableNumber)
        => new(Guid.NewGuid(), tableNumber, DateTimeOffset.UtcNow, OrderStatus.Pending);

    public void MarkPaid()
    {
        if (Status == OrderStatus.Cancelled) throw new InvalidOperationException("Cannot pay a cancelled order.");
        Status = OrderStatus.Paid;
    }

    public void Cancel()
    {
        if (Status == OrderStatus.Paid) throw new InvalidOperationException("Cannot cancel a paid order.");
        Status = OrderStatus.Cancelled;
    }
}

public enum OrderStatus
{
    Pending = 0,
    Paid = 1,
    Cancelled = 2
}
