namespace QrFoodOrdering.Api.Infrastructure;

public sealed class RuntimeEnvironmentOptions
{
    public const string SectionName = "Runtime";

    public bool EnableSwagger { get; init; }
    public bool EnableFrontendDevCors { get; init; }
    public bool ApplyMigrationsOnStartup { get; init; }
    public bool SeedDemoDataOnStartup { get; init; }
}
