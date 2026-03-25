namespace QrFoodOrdering.Domain.Common;

public static class DomainErrorCodes
{
    public const string OrderIdRequired = "ORDER_ID_REQUIRED";
    public const string TableIdRequired = "TABLE_ID_REQUIRED";
    public const string ItemRequired = "ITEM_REQUIRED";
    public const string OrderAlreadyCompleted = "ORDER_ALREADY_COMPLETED";
    public const string OrderNotOpen = "ORDER_NOT_OPEN";

    public const string OrderItemIdRequired = "ORDER_ITEM_ID_REQUIRED";
    public const string ProductNameRequired = "PRODUCT_NAME_REQUIRED";
    public const string QuantityInvalid = "QUANTITY_INVALID";
    public const string UnitPriceRequired = "UNIT_PRICE_REQUIRED";

    public const string MoneyAmountNegative = "MONEY_AMOUNT_NEGATIVE";
    public const string CurrencyRequired = "CURRENCY_REQUIRED";
    public const string MoneyRequired = "MONEY_REQUIRED";
    public const string CurrencyMismatch = "CURRENCY_MISMATCH";

    public const string TableCodeRequired = "TABLE_CODE_REQUIRED";
    public const string TableAlreadyInactive = "TABLE_ALREADY_INACTIVE";
    public const string TableAlreadyActive = "TABLE_ALREADY_ACTIVE";
    public const string TableInactive = "TABLE_INACTIVE";
}
