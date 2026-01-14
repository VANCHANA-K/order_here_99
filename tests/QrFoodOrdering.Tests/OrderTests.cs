using QrFoodOrdering.Domain.Entities;
using Xunit;

namespace QrFoodOrdering.Tests;

public class OrderTests
{
    [Fact]
    public void CreateNew_Should_CreatePendingOrder()
    {
        var order = Order.CreateNew(tableNumber: 1);

        Assert.NotEqual(Guid.Empty, order.Id);
        Assert.Equal(1, order.TableNumber);
        Assert.Equal(OrderStatus.Pending, order.Status);
    }

    [Fact]
    public void MarkPaid_Should_SetPaid()
    {
        var order = Order.CreateNew(1);
        order.MarkPaid();

        Assert.Equal(OrderStatus.Paid, order.Status);
    }
}
