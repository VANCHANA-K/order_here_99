using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Any;

namespace QrFoodOrdering.Api.Swagger;

internal sealed class OpenApiCommonExamples : IOpenApiExampleCatalog
{
    public IReadOnlyDictionary<OpenApiExampleKey, IOpenApiAny> Examples =>
        new Dictionary<OpenApiExampleKey, IOpenApiAny>
        {
            [OpenApiExampleKey.Get(OpenApiRoutePaths.Health, StatusCodes.Status200OK)] =
                OpenApiExampleRegistry.Object(("status", "ok")),
            [OpenApiExampleKey.Get(OpenApiRoutePaths.HealthLive, StatusCodes.Status200OK)] =
                OpenApiExampleRegistry.Object(("status", "ok")),
            [OpenApiExampleKey.Get(OpenApiRoutePaths.HealthReady, StatusCodes.Status200OK)] =
                OpenApiExampleRegistry.Object(("status", "ok")),
            [OpenApiExampleKey.Get(OpenApiRoutePaths.HealthReady, StatusCodes.Status503ServiceUnavailable)] =
                OpenApiExampleRegistry.Error("SERVICE_UNAVAILABLE", "Service is not ready."),
        };
}
