namespace QrFoodOrdering.Api.Models;

public sealed class ApiErrorResponse
{
    public string Code { get; init; } = default!;
    public string Message { get; init; } = default!;
    public string? TraceId { get; init; }
}
