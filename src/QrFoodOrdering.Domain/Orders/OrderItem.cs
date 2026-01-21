using QrFoodOrdering.Domain.Common;

namespace QrFoodOrdering.Domain.Orders;

public class OrderItem
{
    public Guid Id { get; }
    public string ProductName { get; }
    public int Quantity { get; }
    public Money UnitPrice { get; }

    public Money TotalPrice => new(UnitPrice.Amount * Quantity, UnitPrice.Currency);

    public OrderItem(Guid id, string productName, int quantity, Money unitPrice)
    {
        if (id == Guid.Empty)
            throw new DomainException("OrderItem id is required");

        if (string.IsNullOrWhiteSpace(productName))
            throw new DomainException("Product name is required");

        if (quantity <= 0)
            throw new DomainException("Quantity must be greater than zero");

        UnitPrice = unitPrice ?? throw new DomainException("Unit price is required");

        Id = id;
        ProductName = productName;
        Quantity = quantity;
    }
}
