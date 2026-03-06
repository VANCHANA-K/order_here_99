using QrFoodOrdering.Domain.Orders;
using Xunit;

namespace QrFoodOrdering.Tests;

public class OrderTests
{
    [Fact]
    public void Create_order_should_start_as_pending()
    {
        var order = new Order(Guid.NewGuid(), Guid.NewGuid());

        Assert.NotEqual(Guid.Empty, order.Id);
        Assert.Equal(OrderStatus.Pending, order.Status);
        Assert.Empty(order.Items);
    }

    [Fact]
    public void MarkPaid_Should_SetConfirmed()
    {
        var order = new Order(Guid.NewGuid(), Guid.NewGuid());
        order.MarkPaid();

        Assert.Equal(OrderStatus.Confirmed, order.Status);
    }
}
