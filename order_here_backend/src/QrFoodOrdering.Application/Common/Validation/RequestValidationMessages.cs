namespace QrFoodOrdering.Application.Common.Validation;

public static class RequestValidationMessages
{
    public const string InvalidRequest = "Invalid request.";
    public const string RequestBodyRequired = "Request body is required.";
    public const string InvalidJson = "Request body contains invalid JSON.";
    public const string TableIdRequired = "TableId is required.";
    public const string TableIdInvalid = "TableId must be a valid GUID.";
    public const string MenuItemIdInvalid = "MenuItemId must be a valid GUID.";
    public const string MenuItemIdRequired = "MenuItemId is required.";
    public const string ProductNameRequired = "ProductName is required.";
    public const string QuantityMustBeGreaterThanZero = "Quantity must be greater than 0.";
    public const string UnitPriceMustBePositive = "UnitPrice must be positive.";
    public const string EmptyItems = "At least one item is required.";
    public const string TableCodeRequired = "Table code is required";
}
