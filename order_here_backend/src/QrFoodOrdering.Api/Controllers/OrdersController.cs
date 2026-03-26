using Microsoft.AspNetCore.Mvc;
using QrFoodOrdering.Api.Contracts.Common;
using QrFoodOrdering.Api.Contracts.Orders;
using QrFoodOrdering.Api.Infrastructure;
using QrFoodOrdering.Application.Common.Errors;
using QrFoodOrdering.Application.Orders.AddItem;
using QrFoodOrdering.Application.Orders.CloseOrder;
using QrFoodOrdering.Application.Orders.CreateOrder;
using QrFoodOrdering.Application.Orders.CreateOrderViaQr;
using QrFoodOrdering.Application.Orders.GetOrder;
using QrFoodOrdering.Application.Common.Exceptions;

namespace QrFoodOrdering.Api.Controllers;

[ApiController]
[Route("api/v1/orders")]
[Produces("application/json")]
public sealed class OrdersController : ControllerBase
{
    private readonly CreateOrderHandler _createOrderHandler;
    private readonly AddItemHandler _addItemHandler;
    private readonly CreateOrderViaQrHandler _createOrderViaQrHandler;
    private readonly GetOrderHandler _getOrderHandler;
    private readonly CloseOrderHandler _closeOrderHandler;
    private readonly IInFlightRequestGate _requestGate;

    public OrdersController(
        CreateOrderHandler createOrderHandler,
        AddItemHandler addItemHandler,
        CreateOrderViaQrHandler createOrderViaQrHandler,
        GetOrderHandler getOrderHandler,
        CloseOrderHandler closeOrderHandler,
        IInFlightRequestGate requestGate
    )
    {
        _createOrderHandler = createOrderHandler;
        _addItemHandler = addItemHandler;
        _createOrderViaQrHandler = createOrderViaQrHandler;
        _getOrderHandler = getOrderHandler;
        _closeOrderHandler = closeOrderHandler;
        _requestGate = requestGate;
    }

    [HttpPost]
    [ProducesResponseType(typeof(CreateOrderResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CreateOrderResponse>> Create(
        [FromBody] QrFoodOrdering.Api.Contracts.Orders.CreateOrderRequest request,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        CancellationToken ct
    )
    {
        var gateKey = string.IsNullOrWhiteSpace(idempotencyKey)
            ? string.Empty
            : $"orders:create:{idempotencyKey}";

        var result = await _requestGate.ExecuteAsync(
            gateKey,
            token => _createOrderHandler.Handle(new CreateOrderCommand(request.TableId, idempotencyKey), token),
            ct
        );

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.OrderId },
            new CreateOrderResponse(result.OrderId)
        );
    }

    [HttpPost("{id:guid}/items")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddItem(
        Guid id,
        [FromBody] AddItemRequest request,
        // 🔹 Sprint 2 — Day 4 (เตรียมไว้): Idempotency-Key สำหรับ AddItem
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        CancellationToken ct
    )
    {
        await _addItemHandler.Handle(
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
    [ProducesResponseType(typeof(CreateOrderViaQrResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CreateOrderViaQrResponse>> CreateViaQr(
        [FromBody] CreateOrderViaQrRequest request,
        CancellationToken ct)
    {
        var cmd = new CreateOrderViaQrCommand(
            request.TableId,
            request.Items.Select(x => new CreateOrderViaQrItem(x.MenuItemId, x.Quantity)).ToList(),
            request.IdempotencyKey
        );

        var result = await _createOrderViaQrHandler.Handle(cmd, ct);

        return Ok(new CreateOrderViaQrResponse(
            result.OrderId,
            OrderStatusResponseMapper.ToResponseStatus(result.Status),
            result.CreatedAtUtc
        ));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<OrderResponse>> GetById(
        Guid id,
        CancellationToken ct
    )
    {
        var result =
            await _getOrderHandler.Handle(new GetOrderQuery(id), ct)
            ?? throw new NotFoundException(
                ApplicationErrorCodes.OrderNotFound,
                "Order not found"
            );

        return Ok(new OrderResponse(
            result.OrderId,
            OrderStatusResponseMapper.ToResponseStatus(result.Status),
            result.TotalAmount
        ));
    }

    [HttpPost("{id:guid}/close")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Close(
        Guid id,
        CancellationToken ct
    )
    {
        await _closeOrderHandler.Handle(new CloseOrderCommand(id), ct);
        return NoContent();
    }
}
