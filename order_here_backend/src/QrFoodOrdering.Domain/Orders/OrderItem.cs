using QrFoodOrdering.Domain.Common;

namespace QrFoodOrdering.Domain.Orders;

public class OrderItem
{
    public Guid Id { get; private set; }

    public string ProductName { get; private set; } = default!;

    public int Quantity { get; private set; }

    public Money UnitPrice { get; private set; } = default!;

    public Money TotalPrice =>
        new(UnitPrice.Amount * Quantity, UnitPrice.Currency);

    // 🔒 Constructor for EF Core only
    // EF จะใช้ constructor นี้ตอน materialize object
    // ❌ ห้ามมี logic
    private OrderItem() { }

    public OrderItem(
        Guid id,
        string productName,
        int quantity,
        Money unitPrice)
    {
        if (id == Guid.Empty)
            throw new DomainException(
                DomainErrorCodes.OrderItemIdRequired,
                "Order item id is required"
            );

        if (string.IsNullOrWhiteSpace(productName))
            throw new DomainException(
                DomainErrorCodes.ProductNameRequired,
                "Product name is required"
            );

        if (quantity <= 0)
            throw new DomainException(
                DomainErrorCodes.QuantityInvalid,
                "Quantity must be greater than zero"
            );

        UnitPrice = unitPrice
            ?? throw new DomainException(
                DomainErrorCodes.UnitPriceRequired,
                "Unit price is required"
            );

        Id = id;
        ProductName = productName;
        Quantity = quantity;
    }
}
