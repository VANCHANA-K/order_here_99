using QrFoodOrdering.Domain.Orders;
using Xunit;

namespace QrFoodOrdering.UnitTests;

public class OrderUnitTests
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

    [Fact]
    public void MarkPaid_WhenAlreadyConfirmed_Should_Throw()
    {
        var order = new Order(Guid.NewGuid(), Guid.NewGuid(), OrderStatus.Confirmed);

        var ex = Assert.Throws<QrFoodOrdering.Domain.Common.DomainException>(() => order.MarkPaid());

        Assert.Equal(QrFoodOrdering.Domain.Common.DomainErrorCodes.OrderAlreadyConfirmed, ex.ErrorCode);
    }
}
