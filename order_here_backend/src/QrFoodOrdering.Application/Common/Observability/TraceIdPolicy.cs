namespace QrFoodOrdering.Application.Common.Observability;

public static class TraceIdPolicy
{
    public static string Resolve(string? currentTraceId, string source)
    {
        if (!string.IsNullOrWhiteSpace(currentTraceId) && !string.Equals(currentTraceId, "unknown", StringComparison.OrdinalIgnoreCase))
            return currentTraceId;

        var normalizedSource = string.IsNullOrWhiteSpace(source) ? "background" : source.Trim().ToLowerInvariant();
        return $"bg-{normalizedSource}-{Guid.NewGuid():N}";
    }
}
