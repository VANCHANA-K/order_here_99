using System.ComponentModel.DataAnnotations;
using QrFoodOrdering.Api.Validation;
using QrFoodOrdering.Application.Common.Validation;

namespace QrFoodOrdering.Api.Contracts.Orders;

public sealed class CreateOrderViaQrRequest
{
    public CreateOrderViaQrRequest() { }

    public CreateOrderViaQrRequest(
        Guid tableId,
        List<CreateOrderViaQrItemRequest> items,
        string? idempotencyKey
    )
    {
        TableId = tableId;
        Items = items;
        IdempotencyKey = idempotencyKey;
    }

    [NotEmptyGuid(ErrorMessage = RequestValidationMessages.TableIdRequired)]
    public Guid TableId { get; init; }

    [Required]
    [MinLength(1, ErrorMessage = RequestValidationMessages.EmptyItems)]
    public List<CreateOrderViaQrItemRequest> Items { get; init; } = [];

    public string? IdempotencyKey { get; init; }
}

public sealed class CreateOrderViaQrItemRequest
{
    public CreateOrderViaQrItemRequest() { }

    public CreateOrderViaQrItemRequest(Guid menuItemId, int quantity)
    {
        MenuItemId = menuItemId;
        Quantity = quantity;
    }

    [NotEmptyGuid(ErrorMessage = RequestValidationMessages.MenuItemIdRequired)]
    public Guid MenuItemId { get; init; }

    [Range(1, int.MaxValue, ErrorMessage = RequestValidationMessages.QuantityMustBeGreaterThanZero)]
    public int Quantity { get; init; }
}
