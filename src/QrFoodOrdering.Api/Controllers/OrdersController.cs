using Microsoft.AspNetCore.Mvc;
using QrFoodOrdering.Api.Contracts.Orders;
using QrFoodOrdering.Application.Orders.AddItem;
using QrFoodOrdering.Application.Orders.CloseOrder;
using QrFoodOrdering.Application.Orders.CreateOrder;
using QrFoodOrdering.Application.Orders.GetOrder;

namespace QrFoodOrdering.Api.Controllers;

[ApiController]
[Route("orders")]
public sealed class OrdersController : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<CreateOrderResponse>> Create(
        [FromServices] CreateOrderHandler handler,
        CancellationToken ct)
    {
        var result = await handler.Handle(new CreateOrderCommand(), ct);

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.OrderId },
            new CreateOrderResponse(result.OrderId));
    }

    [HttpPost("{id:guid}/items")]
    public async Task<IActionResult> AddItem(
        Guid id,
        [FromBody] AddItemRequest request,
        [FromServices] AddItemHandler handler,
        CancellationToken ct)
    {
        await handler.Handle(
            new AddItemCommand(
                id,
                request.ProductName,
                request.Quantity,
                request.UnitPrice),
            ct);

        return NoContent();
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrderResponse>> GetById(
        Guid id,
        [FromServices] GetOrderHandler handler,
        CancellationToken ct)
    {
        var result = await handler.Handle(id, ct);
        if (result is null) return NotFound();

        return Ok(new OrderResponse(
            result.OrderId,
            result.Status,
            result.TotalAmount));
    }

    [HttpPost("{id:guid}/close")]
    public async Task<IActionResult> Close(
        Guid id,
        [FromServices] CloseOrderHandler handler,
        CancellationToken ct)
    {
        await handler.Handle(id, ct);
        return NoContent();
    }
}
