namespace QrFoodOrdering.Api.Swagger;

internal readonly record struct OpenApiExampleKey(string Method, string Path, int StatusCode)
{
    public static OpenApiExampleKey Get(string path, int statusCode) => new("GET", path, statusCode);

    public static OpenApiExampleKey Post(string path, int statusCode) => new("POST", path, statusCode);

    public static OpenApiExampleKey Patch(string path, int statusCode) => new("PATCH", path, statusCode);

    public static OpenApiExampleKey From(string method, string path, string statusCode) =>
        new(method.ToUpperInvariant(), path, int.Parse(statusCode));
}
