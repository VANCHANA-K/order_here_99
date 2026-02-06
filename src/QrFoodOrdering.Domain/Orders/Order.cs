using QrFoodOrdering.Domain.Common;

namespace QrFoodOrdering.Domain.Orders;

public class Order
{
    private readonly List<OrderItem> _items = new();

    public Guid Id { get; }
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
    public Order(Guid id, DateTime? createdAtUtc = null)
    {
        if (id == Guid.Empty)
            throw new DomainException("Order id is required");

        Id = id;
        Status = OrderStatus.Created;
        CreatedAtUtc = createdAtUtc ?? DateTime.UtcNow;
    }

    public void AddItem(OrderItem item)
    {
        EnsureOrderIsOpen(); // R1
        _items.Add(item ?? throw new DomainException("Item is required"));
    }

    public void Close()
    {
        EnsureOrderIsOpen();
        Status = OrderStatus.Closed;
    }

    public void Cancel()
    {
        // R2: Cancel หลัง Paid ไม่ได้
        if (Status == OrderStatus.Paid)
            throw new DomainException("Cannot cancel a paid order");

        if (Status == OrderStatus.Closed)
            throw new DomainException("Cannot cancel a closed order");

        Status = OrderStatus.Cancelled;
    }

    // เตรียมไว้รองรับ flow ในอนาคต
    public void MarkPaid()
    {
        EnsureOrderIsOpen();
        Status = OrderStatus.Paid;
    }

    private void EnsureOrderIsOpen()
    {
        if (Status == OrderStatus.Closed || Status == OrderStatus.Cancelled)
            throw new DomainException("Order is not open");
    }
}


