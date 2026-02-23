using System.Text.Json.Serialization;

namespace QrFoodOrdering.Api.Contracts.Common;

public sealed record ApiErrorResponse(
    [property: JsonPropertyName("errorCode")] string ErrorCode,
    [property: JsonPropertyName("message")] string Message,
    [property: JsonPropertyName("traceId")] string TraceId
);
