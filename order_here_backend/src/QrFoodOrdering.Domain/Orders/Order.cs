using QrFoodOrdering.Domain.Common;

namespace QrFoodOrdering.Domain.Orders;

public class Order
{
    private readonly List<OrderItem> _items = new();

    public Guid Id { get; }
    public Guid TableId { get; private set; }
    public OrderStatus Status { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    public Money TotalAmount
    {
        get
        {
            var total = Money.Zero("THB");
            foreach (var item in _items)
                total = total.Add(item.TotalPrice);
            return total;
        }
    }

    // 🔒 Constructor for EF Core only
    // EF ใช้ constructor นี้ตอน materialize object จาก database
    // ❌ ห้ามมี logic
    // ❌ ห้าม set Id / Status / Date
    private Order()
    {
        _items = new List<OrderItem>();
    }

    public Order(Guid id, Guid tableId, DateTime? createdAtUtc = null)
        : this(id, tableId, OrderStatus.Pending, createdAtUtc)
    {
    }

    public Order(Guid id, Guid tableId, OrderStatus status, DateTime? createdAtUtc = null)
    {
        if (id == Guid.Empty)
            throw new DomainException(DomainErrorCodes.OrderIdRequired, "Order id is required");

        if (tableId == Guid.Empty)
            throw new DomainException(DomainErrorCodes.TableIdRequired, "Table id is required");

        Id = id;
        TableId = tableId;
        Status = status;
        CreatedAtUtc = createdAtUtc ?? DateTime.UtcNow;
    }

    public void AddItem(OrderItem item)
    {
        EnsureOrderIsOpen(); // R1
        _items.Add(
            item ?? throw new DomainException(DomainErrorCodes.ItemRequired, "Item is required")
        );
    }

    public void Close()
    {
        EnsureOrderIsOpen();
        Status = OrderStatus.Completed;
    }

    public void Cancel()
    {
        if (Status == OrderStatus.Completed)
            throw new DomainException(
                DomainErrorCodes.OrderAlreadyCompleted,
                "Cannot cancel a completed order"
            );

        Status = OrderStatus.Cancelled;
    }

    // เตรียมไว้รองรับ flow ในอนาคต
    public void MarkPaid()
    {
        EnsureOrderIsOpen();
        Status = OrderStatus.Confirmed;
    }

    private void EnsureOrderIsOpen()
    {
        if (Status == OrderStatus.Completed || Status == OrderStatus.Cancelled)
            throw new DomainException(DomainErrorCodes.OrderNotOpen, "Order is not open");
    }
}
