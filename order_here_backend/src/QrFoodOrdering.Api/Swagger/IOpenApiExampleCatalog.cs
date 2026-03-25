using Microsoft.OpenApi.Any;

namespace QrFoodOrdering.Api.Swagger;

internal interface IOpenApiExampleCatalog
{
    IReadOnlyDictionary<OpenApiExampleKey, IOpenApiAny> Examples { get; }
}
