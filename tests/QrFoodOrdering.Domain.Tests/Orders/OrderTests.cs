using QrFoodOrdering.Domain.Common;
using QrFoodOrdering.Domain.Orders;
using Xunit;

namespace QrFoodOrdering.Domain.Tests.Orders;

public class OrderTests
{
    [Fact]
    public void Create_order_should_start_as_created()
    {
        var order = new Order(Guid.NewGuid());
        Assert.Equal(OrderStatus.Created, order.Status);
        Assert.Empty(order.Items);
    }

    [Fact]
    public void AddItem_after_close_should_throw()
    {
        var order = new Order(Guid.NewGuid());
        order.Close();

        var item = new OrderItem(Guid.NewGuid(), "Fried Rice", 1, new Money(50));

        var ex = Assert.Throws<DomainException>(() => order.AddItem(item));
        Assert.Contains("not open", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AddItem_after_cancel_should_throw()
    {
        var order = new Order(Guid.NewGuid());
        order.Cancel();

        var item = new OrderItem(Guid.NewGuid(), "Pad Thai", 1, new Money(60));

        Assert.Throws<DomainException>(() => order.AddItem(item));
    }

    [Fact]
    public void Cancel_after_paid_should_throw()
    {
        var order = new Order(Guid.NewGuid());
        order.MarkPaid();

        var ex = Assert.Throws<DomainException>(() => order.Cancel());
        Assert.Contains("paid", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TotalAmount_should_sum_items_correctly()
    {
        var order = new Order(Guid.NewGuid());
        order.AddItem(new OrderItem(Guid.NewGuid(), "Coffee", 2, new Money(40))); // 80
        order.AddItem(new OrderItem(Guid.NewGuid(), "Cake", 1, new Money(90)));   // 90

        Assert.Equal(170m, order.TotalAmount.Amount);
        Assert.Equal("THB", order.TotalAmount.Currency);
    }
}
