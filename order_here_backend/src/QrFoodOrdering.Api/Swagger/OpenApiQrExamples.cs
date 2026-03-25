using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Any;

namespace QrFoodOrdering.Api.Swagger;

internal sealed class OpenApiQrExamples : IOpenApiExampleCatalog
{
    public IReadOnlyDictionary<OpenApiExampleKey, IOpenApiAny> Examples =>
        new Dictionary<OpenApiExampleKey, IOpenApiAny>
        {
            [OpenApiExampleKey.Post(OpenApiRoutePaths.TableQr, StatusCodes.Status200OK)] = OpenApiExampleRegistry.Object(
                ("tableId", "66666666-6666-6666-6666-666666666666"),
                ("token", "qr-token-abc123"),
                ("qrUrl", "https://localhost:3000/order/qr-token-abc123"),
                ("generatedAtUtc", "2026-03-25T12:00:00Z")
            ),
            [OpenApiExampleKey.Post(OpenApiRoutePaths.TableQr, StatusCodes.Status404NotFound)] =
                OpenApiExampleRegistry.Error("TABLE_NOT_FOUND", "Table not found."),
            [OpenApiExampleKey.Post(OpenApiRoutePaths.TableQr, StatusCodes.Status409Conflict)] =
                OpenApiExampleRegistry.Error("TABLE_INACTIVE", "Cannot generate QR for inactive table."),
            [OpenApiExampleKey.Get(OpenApiRoutePaths.QrResolve, StatusCodes.Status200OK)] = OpenApiExampleRegistry.Object(
                ("tableId", "77777777-7777-7777-7777-777777777777"),
                ("tableCode", "B01")
            ),
            [OpenApiExampleKey.Get(OpenApiRoutePaths.QrResolve, StatusCodes.Status400BadRequest)] =
                OpenApiExampleRegistry.Error("QR_INVALID", "QR token is required."),
            [OpenApiExampleKey.Get(OpenApiRoutePaths.QrResolve, StatusCodes.Status404NotFound)] =
                OpenApiExampleRegistry.Error("QR_NOT_FOUND", "QR code was not found."),
            [OpenApiExampleKey.Get(OpenApiRoutePaths.QrResolve, StatusCodes.Status409Conflict)] =
                OpenApiExampleRegistry.Error("QR_INACTIVE", "This QR code is inactive."),
        };
}
