namespace QrFoodOrdering.Application.Tables.GetAll;

public sealed record GetAllTablesResult(
    Guid Id,
    string Code,
    string Status
);
