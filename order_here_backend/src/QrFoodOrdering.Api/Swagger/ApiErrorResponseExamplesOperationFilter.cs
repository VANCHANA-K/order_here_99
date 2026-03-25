using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace QrFoodOrdering.Api.Swagger;

public sealed class ApiErrorResponseExamplesOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var relativePath = "/" + (context.ApiDescription.RelativePath ?? string.Empty).Trim('/');
        var method = context.ApiDescription.HttpMethod?.ToUpperInvariant() ?? "GET";

        foreach (var response in operation.Responses)
        {
            if (!response.Value.Content.TryGetValue("application/json", out var mediaType))
                continue;

            var example = OpenApiExampleRegistry.TryGet(method, relativePath, response.Key);
            if (example is null)
                continue;

            mediaType.Example = example;
        }
    }
}
