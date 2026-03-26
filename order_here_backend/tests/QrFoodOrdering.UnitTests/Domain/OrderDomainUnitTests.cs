using QrFoodOrdering.Domain.Common;
using QrFoodOrdering.Domain.Orders;
using Xunit;

namespace QrFoodOrdering.UnitTests.Domain;

public class OrderDomainUnitTests
{
    [Fact]
    public void Create_order_should_start_as_pending()
    {
        var order = new Order(Guid.NewGuid(), Guid.NewGuid());
        Assert.Equal(OrderStatus.Pending, order.Status);
        Assert.Empty(order.Items);
    }

    [Fact]
    public void AddItem_after_close_should_throw()
    {
        var order = new Order(Guid.NewGuid(), Guid.NewGuid());
        order.Close();

        var item = new OrderItem(Guid.NewGuid(), "Fried Rice", 1, new Money(50));

        var ex = Assert.Throws<DomainException>(() => order.AddItem(item));
        Assert.Equal(DomainErrorCodes.OrderNotOpen, ex.ErrorCode);
        Assert.Contains("not open", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AddItem_after_cancel_should_throw()
    {
        var order = new Order(Guid.NewGuid(), Guid.NewGuid());
        order.Cancel();

        var item = new OrderItem(Guid.NewGuid(), "Pad Thai", 1, new Money(60));

        var ex = Assert.Throws<DomainException>(() => order.AddItem(item));
        Assert.Equal(DomainErrorCodes.OrderNotOpen, ex.ErrorCode);
    }

    [Fact]
    public void AddItem_after_cooking_should_throw_items_locked()
    {
        var order = new Order(Guid.NewGuid(), Guid.NewGuid(), OrderStatus.Cooking);

        var item = new OrderItem(Guid.NewGuid(), "Pad Thai", 1, new Money(60));

        var ex = Assert.Throws<DomainException>(() => order.AddItem(item));
        Assert.Equal(DomainErrorCodes.OrderItemsLocked, ex.ErrorCode);
    }

    [Fact]
    public void Cancel_after_completed_should_throw()
    {
        var order = new Order(Guid.NewGuid(), Guid.NewGuid());
        order.Close();

        var ex = Assert.Throws<DomainException>(() => order.Cancel());
        Assert.Equal(DomainErrorCodes.OrderAlreadyCompleted, ex.ErrorCode);
        Assert.Contains("completed", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Cancel_after_cancelled_should_throw()
    {
        var order = new Order(Guid.NewGuid(), Guid.NewGuid());
        order.Cancel();

        var ex = Assert.Throws<DomainException>(() => order.Cancel());
        Assert.Equal(DomainErrorCodes.OrderAlreadyCancelled, ex.ErrorCode);
    }

    [Fact]
    public void Cancel_after_cooking_should_throw()
    {
        var order = new Order(Guid.NewGuid(), Guid.NewGuid(), OrderStatus.Cooking);

        var ex = Assert.Throws<DomainException>(() => order.Cancel());
        Assert.Equal(DomainErrorCodes.OrderCannotBeCancelled, ex.ErrorCode);
    }

    [Fact]
    public void MarkPaid_after_ready_should_throw()
    {
        var order = new Order(Guid.NewGuid(), Guid.NewGuid(), OrderStatus.Ready);

        var ex = Assert.Throws<DomainException>(() => order.MarkPaid());
        Assert.Equal(DomainErrorCodes.OrderCannotBeConfirmed, ex.ErrorCode);
    }

    [Fact]
    public void TotalAmount_should_sum_items_correctly()
    {
        var order = new Order(Guid.NewGuid(), Guid.NewGuid());
        order.AddItem(new OrderItem(Guid.NewGuid(), "Coffee", 2, new Money(40))); // 80
        order.AddItem(new OrderItem(Guid.NewGuid(), "Cake", 1, new Money(90)));   // 90

        Assert.Equal(170m, order.TotalAmount.Amount);
        Assert.Equal("THB", order.TotalAmount.Currency);
    }
}
