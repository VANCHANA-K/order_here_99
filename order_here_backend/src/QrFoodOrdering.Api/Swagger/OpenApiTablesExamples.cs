using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Any;

namespace QrFoodOrdering.Api.Swagger;

internal sealed class OpenApiTablesExamples : IOpenApiExampleCatalog
{
    public IReadOnlyDictionary<OpenApiExampleKey, IOpenApiAny> Examples =>
        new Dictionary<OpenApiExampleKey, IOpenApiAny>
        {
            [OpenApiExampleKey.Get(OpenApiRoutePaths.Tables, StatusCodes.Status200OK)] = OpenApiExampleRegistry.Array(
                OpenApiExampleRegistry.Object(
                    ("id", "44444444-4444-4444-4444-444444444444"),
                    ("code", "A01"),
                    ("status", "Active")
                )
            ),
            [OpenApiExampleKey.Post(OpenApiRoutePaths.Tables, StatusCodes.Status201Created)] =
                OpenApiExampleRegistry.Object(("id", "55555555-5555-5555-5555-555555555555")),
            [OpenApiExampleKey.Post(OpenApiRoutePaths.Tables, StatusCodes.Status400BadRequest)] =
                OpenApiExampleRegistry.Error("TABLE_CODE_REQUIRED", "Table code is required"),
            [OpenApiExampleKey.Post(OpenApiRoutePaths.Tables, StatusCodes.Status409Conflict)] =
                OpenApiExampleRegistry.Error("TABLE_CODE_ALREADY_EXISTS", "Table code already exists."),
            [OpenApiExampleKey.Post(OpenApiRoutePaths.TableActivate, StatusCodes.Status204NoContent)] =
                OpenApiExampleRegistry.Object(),
            [OpenApiExampleKey.Post(OpenApiRoutePaths.TableActivate, StatusCodes.Status404NotFound)] =
                OpenApiExampleRegistry.Error("TABLE_NOT_FOUND", "Table not found."),
            [OpenApiExampleKey.Post(OpenApiRoutePaths.TableActivate, StatusCodes.Status409Conflict)] =
                OpenApiExampleRegistry.Error("TABLE_ALREADY_ACTIVE", "Table is already active"),
            [OpenApiExampleKey.Patch(OpenApiRoutePaths.TableDisable, StatusCodes.Status204NoContent)] =
                OpenApiExampleRegistry.Object(),
            [OpenApiExampleKey.Patch(OpenApiRoutePaths.TableDisable, StatusCodes.Status404NotFound)] =
                OpenApiExampleRegistry.Error("TABLE_NOT_FOUND", "Table not found."),
            [OpenApiExampleKey.Patch(OpenApiRoutePaths.TableDisable, StatusCodes.Status409Conflict)] =
                OpenApiExampleRegistry.Error("TABLE_ALREADY_INACTIVE", "Table is already inactive"),
        };
}
