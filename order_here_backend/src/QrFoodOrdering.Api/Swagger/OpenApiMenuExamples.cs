using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Any;

namespace QrFoodOrdering.Api.Swagger;

internal sealed class OpenApiMenuExamples : IOpenApiExampleCatalog
{
    public IReadOnlyDictionary<OpenApiExampleKey, IOpenApiAny> Examples =>
        new Dictionary<OpenApiExampleKey, IOpenApiAny>
        {
            [OpenApiExampleKey.Get(OpenApiRoutePaths.TableMenu, StatusCodes.Status200OK)] = OpenApiExampleRegistry.Array(
                OpenApiExampleRegistry.Object(
                    ("id", "88888888-8888-8888-8888-888888888888"),
                    ("code", "M001"),
                    ("name", "Pad Thai"),
                    ("price", 60m),
                    ("isAvailable", true)
                )
            ),
            [OpenApiExampleKey.Get(OpenApiRoutePaths.TableMenu, StatusCodes.Status400BadRequest)] =
                OpenApiExampleRegistry.Error("TABLE_ID_REQUIRED", "TableId is required."),
            [OpenApiExampleKey.Get(OpenApiRoutePaths.TableMenu, StatusCodes.Status404NotFound)] =
                OpenApiExampleRegistry.Error("TABLE_NOT_FOUND", "Table not found."),
        };
}
