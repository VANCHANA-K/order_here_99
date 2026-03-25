using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace QrFoodOrdering.Api.Swagger;

public static class OpenApiExampleRegistry
{
    private static readonly IReadOnlyList<IOpenApiExampleCatalog> Catalogs =
    [
        new OpenApiCommonExamples(),
        new OpenApiOrdersExamples(),
        new OpenApiTablesExamples(),
        new OpenApiQrExamples(),
        new OpenApiMenuExamples(),
    ];

    private static readonly Dictionary<OpenApiExampleKey, IOpenApiAny> Examples =
        Catalogs
            .SelectMany(x => x.Examples)
            .ToDictionary(x => x.Key, x => x.Value);

    public static IOpenApiAny? TryGet(string method, string path, string statusCode) =>
        Examples.TryGetValue(OpenApiExampleKey.From(method, path, statusCode), out var example)
            ? example
            : statusCode == StatusCodes.Status500InternalServerError.ToString()
                ? Error("UNEXPECTED_ERROR", "Unexpected error occurred.")
                : null;

    internal static OpenApiObject Error(string errorCode, string message) =>
        Object(
            ("errorCode", errorCode),
            ("message", message),
            ("traceId", "4f3d2c1b0a9e87654321fedcba098765")
        );

    internal static OpenApiObject Object(params (string Key, object Value)[] pairs)
    {
        var result = new OpenApiObject();
        foreach (var (key, value) in pairs)
        {
            result[key] = value switch
            {
                string s => new OpenApiString(s),
                decimal d => new OpenApiDouble((double)d),
                int i => new OpenApiInteger(i),
                bool b => new OpenApiBoolean(b),
                IOpenApiAny any => any,
                _ => new OpenApiString(value.ToString() ?? string.Empty),
            };
        }

        return result;
    }

    internal static OpenApiArray Array(params OpenApiObject[] items)
    {
        var result = new OpenApiArray();
        foreach (var item in items)
            result.Add(item);

        return result;
    }
}
