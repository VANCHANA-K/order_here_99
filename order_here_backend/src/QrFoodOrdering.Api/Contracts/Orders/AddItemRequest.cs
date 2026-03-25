using System.ComponentModel.DataAnnotations;
using QrFoodOrdering.Application.Common.Validation;

namespace QrFoodOrdering.Api.Contracts.Orders;

public sealed class AddItemRequest
{
    [Required(AllowEmptyStrings = false, ErrorMessage = RequestValidationMessages.ProductNameRequired)]
    public string ProductName { get; init; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = RequestValidationMessages.QuantityMustBeGreaterThanZero)]
    public int Quantity { get; init; }

    [Range(0.01, double.MaxValue, ErrorMessage = RequestValidationMessages.UnitPriceMustBePositive)]
    public decimal UnitPrice { get; init; }
}
