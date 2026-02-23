using QrFoodOrdering.Domain.Orders;
using Xunit;

namespace QrFoodOrdering.Tests;

public class OrderTests
{
    [Fact]
    public void Create_order_should_start_as_created()
    {
        var order = new Order(Guid.NewGuid());

        Assert.NotEqual(Guid.Empty, order.Id);
        Assert.Equal(OrderStatus.Created, order.Status);
        Assert.Empty(order.Items);
    }

    [Fact]
    public void MarkPaid_Should_SetPaid()
    {
        var order = new Order(Guid.NewGuid());
        order.MarkPaid();

        Assert.Equal(OrderStatus.Paid, order.Status);
    }
}
