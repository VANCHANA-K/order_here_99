namespace QrFoodOrdering.Application.Common.Observability;

public static class LogActions
{
    public const string CreateOrder = "CREATE_ORDER";
    public const string AddItem = "ADD_ITEM";
}

public static class LogStatuses
{
    public const string Success = "SUCCESS";
    public const string Failed = "FAILED";
    public const string Hit = "HIT";
}
