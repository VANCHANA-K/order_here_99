namespace QrFoodOrdering.Application.Common.Errors;

public static class ApplicationErrorCodes
{
    public const string InvalidRequest = "INVALID_REQUEST";
    public const string ConfigurationInvalid = "CONFIGURATION_INVALID";
    public const string AuditLogImmutable = "AUDIT_LOG_IMMUTABLE";
    public const string OperationTimedOut = "OPERATION_TIMED_OUT";
    public const string DatabaseUnavailable = "DATABASE_UNAVAILABLE";
    public const string IdempotencyKeyRequired = "IDEMPOTENCY_KEY_REQUIRED";
    public const string IdempotencyKeyConflict = "IDEMPOTENCY_KEY_CONFLICT";
    public const string IdempotencyKeyPayloadMismatch = "IDEMPOTENCY_KEY_PAYLOAD_MISMATCH";
    public const string ConcurrencyConflict = "CONCURRENCY_CONFLICT";
    public const string RequestBodyRequired = "REQUEST_BODY_REQUIRED";
    public const string InvalidJson = "INVALID_JSON";

    public const string TableIdRequired = "TABLE_ID_REQUIRED";
    public const string TableIdInvalid = "TABLE_ID_INVALID";
    public const string MenuItemIdInvalid = "MENU_ITEM_ID_INVALID";
    public const string MenuItemIdRequired = "MENU_ITEM_ID_REQUIRED";
    public const string OrderNotFound = "ORDER_NOT_FOUND";
    public const string ProductNameRequired = "PRODUCT_NAME_REQUIRED";
    public const string UnitPriceInvalid = "UNIT_PRICE_INVALID";
    public const string TableNotFound = "TABLE_NOT_FOUND";
    public const string TableInactive = "TABLE_INACTIVE";
    public const string TableCodeAlreadyExists = "TABLE_CODE_ALREADY_EXISTS";

    public const string QrInvalid = "QR_INVALID";
    public const string QrNotFound = "QR_NOT_FOUND";
    public const string QrInactive = "QR_INACTIVE";
    public const string QrExpired = "QR_EXPIRED";
    public const string QrTokenAlreadyExists = "QR_TOKEN_ALREADY_EXISTS";

    public const string EmptyItems = "EMPTY_ITEMS";
    public const string InvalidQuantity = "INVALID_QTY";
    public const string ItemNotFound = "ITEM_NOT_FOUND";
    public const string ItemInactive = "ITEM_INACTIVE";
    public const string ItemUnavailable = "ITEM_UNAVAILABLE";
    public const string MenuCodeAlreadyExists = "MENU_CODE_ALREADY_EXISTS";
}
