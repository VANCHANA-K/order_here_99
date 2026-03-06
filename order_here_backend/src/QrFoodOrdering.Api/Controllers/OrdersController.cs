using Microsoft.AspNetCore.Mvc;
using QrFoodOrdering.Api.Contracts.Orders;
using QrFoodOrdering.Application.Orders.AddItem;
using QrFoodOrdering.Application.Orders.CloseOrder;
using QrFoodOrdering.Application.Orders.CreateOrder;
using QrFoodOrdering.Application.Orders.CreateOrderViaQr;
using QrFoodOrdering.Application.Orders.GetOrder;
using QrFoodOrdering.Application.Common.Exceptions;

namespace QrFoodOrdering.Api.Controllers;

[ApiController]
[Route("api/v1/orders")]
public sealed class OrdersController : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<CreateOrderResponse>> Create(
        [FromBody] QrFoodOrdering.Api.Contracts.Orders.CreateOrderRequest request,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        [FromServices] CreateOrderHandler handler,
        CancellationToken ct
    )
    {
        if (request.TableId == Guid.Empty)
            throw new InvalidRequestException("TABLE_ID_REQUIRED", "TableId is required.");

        var result = await handler.Handle(
            new CreateOrderCommand(request.TableId, idempotencyKey),
            ct
        );

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.OrderId },
            new CreateOrderResponse(result.OrderId)
        );
    }

    [HttpPost("{id:guid}/items")]
    public async Task<IActionResult> AddItem(
        Guid id,
        [FromBody] AddItemRequest request,
        // 🔹 Sprint 2 — Day 4 (เตรียมไว้): Idempotency-Key สำหรับ AddItem
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        [FromServices] AddItemHandler handler,
        CancellationToken ct
    )
    {
        await handler.Handle(
            new AddItemCommand(
                id,
                request.ProductName,
                request.Quantity,
                request.UnitPrice,
                idempotencyKey
            ),
            ct
        );

        return NoContent();
    }

    [HttpPost("qr")]
    public async Task<ActionResult<CreateOrderViaQrResponse>> CreateViaQr(
        [FromBody] CreateOrderViaQrRequest request,
        [FromServices] CreateOrderViaQrHandler handler,
        CancellationToken ct)
    {
        var cmd = new CreateOrderViaQrCommand(
            request.TableId,
            request.Items.Select(x => new CreateOrderViaQrItem(x.MenuItemId, x.Quantity)).ToList(),
            request.IdempotencyKey
        );

        var result = await handler.Handle(cmd, ct);

        return Ok(new CreateOrderViaQrResponse(
            result.OrderId,
            result.Status.ToString().ToUpperInvariant(),
            result.CreatedAtUtc
        ));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrderResponse>> GetById(
        Guid id,
        [FromServices] GetOrderHandler handler,
        CancellationToken ct
    )
    {
        var result = await handler.Handle(id, ct) ?? throw new NotFoundException("Order not found");

        return Ok(new OrderResponse(result.OrderId, result.Status, result.TotalAmount));
    }

    [HttpPost("{id:guid}/close")]
    public async Task<IActionResult> Close(
        Guid id,
        [FromServices] CloseOrderHandler handler,
        CancellationToken ct
    )
    {
        await handler.Handle(id, ct);
        return NoContent();
    }
}
