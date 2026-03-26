namespace QrFoodOrdering.Application.Common.Audit;

public static class AuditEvents
{
    public const string HealthCheck = "HEALTH_CHECK";
    public const string TableCreated = "TABLE_CREATED";
    public const string TableStatusChanged = "TABLE_STATUS_CHANGED";
    public const string OrderCreated = "ORDER_CREATED";
    public const string OrderItemAdded = "ORDER_ITEM_ADDED";
    public const string OrderClosed = "ORDER_CLOSED";
    public const string OrderPlacedViaQr = "ORDER_PLACED_VIA_QR";
    public const string QrGenerated = "QR_GENERATED";
    public const string QrResolved = "QR_RESOLVED";
}

public static class AuditEntities
{
    public const string Health = "Health";
    public const string Table = "Table";
    public const string Order = "Order";
    public const string QrCode = "QrCode";
}
