namespace QrFoodOrdering.Infrastructure.Audit;

public sealed class AuditLogOptions
{
    public const string SectionName = "AuditLogs";

    public string DirectoryPath { get; init; } = "data/audit";
    public int RetentionDays { get; init; } = 30;
    public string AggregationPeriod { get; init; } = "Daily";
}
