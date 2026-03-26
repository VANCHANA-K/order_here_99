using System.Net;
using System.Text.Json;
using QrFoodOrdering.IntegrationTests.Infrastructure;

namespace QrFoodOrdering.IntegrationTests;

public sealed class SwaggerApiIntegrationTests
{
    [Fact]
    public async Task Swagger_document_should_include_api_error_response_examples()
    {
        await using var factory = new TestApiFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/swagger/v1/swagger.json");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        await using var stream = await response.Content.ReadAsStreamAsync();
        using var document = await JsonDocument.ParseAsync(stream);
        var root = document.RootElement;
        var paths = root.GetProperty("paths");

        Assert.False(paths.TryGetProperty("/test/error", out _));

        var apiErrorSchema = root
            .GetProperty("components")
            .GetProperty("schemas")
            .GetProperty("ApiErrorResponse");

        Assert.True(apiErrorSchema.TryGetProperty("properties", out var schemaProperties));
        Assert.True(schemaProperties.TryGetProperty("errorCode", out _));
        Assert.True(schemaProperties.TryGetProperty("message", out _));
        Assert.True(schemaProperties.TryGetProperty("traceId", out _));

        var orderGetResponses = root
            .GetProperty("paths")
            .GetProperty("/api/v1/orders/{id}")
            .GetProperty("get")
            .GetProperty("responses");

        var createOrderResponses = root
            .GetProperty("paths")
            .GetProperty("/api/v1/orders")
            .GetProperty("post")
            .GetProperty("responses");

        var createOrderSuccessExample = createOrderResponses
            .GetProperty("201")
            .GetProperty("content")
            .GetProperty("application/json")
            .GetProperty("example");

        Assert.False(string.IsNullOrWhiteSpace(createOrderSuccessExample.GetProperty("orderId").GetString()));

        var orderGetSuccessExample = orderGetResponses
            .GetProperty("200")
            .GetProperty("content")
            .GetProperty("application/json")
            .GetProperty("example");

        Assert.Equal("Pending", orderGetSuccessExample.GetProperty("status").GetString());
        Assert.Equal(120m, orderGetSuccessExample.GetProperty("totalAmount").GetDecimal());

        var notFoundExample = orderGetResponses
            .GetProperty("404")
            .GetProperty("content")
            .GetProperty("application/json")
            .GetProperty("example");

        Assert.Equal("ORDER_NOT_FOUND", notFoundExample.GetProperty("errorCode").GetString());
        Assert.Equal("Order not found", notFoundExample.GetProperty("message").GetString());
        Assert.False(string.IsNullOrWhiteSpace(notFoundExample.GetProperty("traceId").GetString()));

        var unexpectedErrorExample = orderGetResponses
            .GetProperty("500")
            .GetProperty("content")
            .GetProperty("application/json")
            .GetProperty("example");

        Assert.Equal("UNEXPECTED_ERROR", unexpectedErrorExample.GetProperty("errorCode").GetString());
        Assert.Equal("Unexpected error occurred.", unexpectedErrorExample.GetProperty("message").GetString());
        Assert.False(string.IsNullOrWhiteSpace(unexpectedErrorExample.GetProperty("traceId").GetString()));

        var healthSuccessExample = root
            .GetProperty("paths")
            .GetProperty("/health")
            .GetProperty("get")
            .GetProperty("responses")
            .GetProperty("200")
            .GetProperty("content")
            .GetProperty("application/json")
            .GetProperty("example");

        Assert.Equal("ok", healthSuccessExample.GetProperty("status").GetString());

        var healthLiveSuccessExample = root
            .GetProperty("paths")
            .GetProperty("/health/live")
            .GetProperty("get")
            .GetProperty("responses")
            .GetProperty("200")
            .GetProperty("content")
            .GetProperty("application/json")
            .GetProperty("example");

        Assert.Equal("ok", healthLiveSuccessExample.GetProperty("status").GetString());

        var healthReadyResponses = root
            .GetProperty("paths")
            .GetProperty("/health/ready")
            .GetProperty("get")
            .GetProperty("responses");

        var healthReadySuccessExample = healthReadyResponses
            .GetProperty("200")
            .GetProperty("content")
            .GetProperty("application/json")
            .GetProperty("example");

        Assert.Equal("ok", healthReadySuccessExample.GetProperty("status").GetString());

        var healthReadyUnavailableExample = healthReadyResponses
            .GetProperty("503")
            .GetProperty("content")
            .GetProperty("application/json")
            .GetProperty("example");

        Assert.Equal("SERVICE_UNAVAILABLE", healthReadyUnavailableExample.GetProperty("errorCode").GetString());

        var tablesListSuccessExample = root
            .GetProperty("paths")
            .GetProperty("/api/v1/tables")
            .GetProperty("get")
            .GetProperty("responses")
            .GetProperty("200")
            .GetProperty("content")
            .GetProperty("application/json")
            .GetProperty("example");

        Assert.Equal("A01", tablesListSuccessExample[0].GetProperty("code").GetString());
        Assert.Equal("Active", tablesListSuccessExample[0].GetProperty("status").GetString());

        var createTableSuccessExample = root
            .GetProperty("paths")
            .GetProperty("/api/v1/tables")
            .GetProperty("post")
            .GetProperty("responses")
            .GetProperty("201")
            .GetProperty("content")
            .GetProperty("application/json")
            .GetProperty("example");

        Assert.False(string.IsNullOrWhiteSpace(createTableSuccessExample.GetProperty("id").GetString()));

        var generateQrSuccessExample = root
            .GetProperty("paths")
            .GetProperty("/api/v1/tables/{id}/qr")
            .GetProperty("post")
            .GetProperty("responses")
            .GetProperty("200")
            .GetProperty("content")
            .GetProperty("application/json")
            .GetProperty("example");

        Assert.Equal(
            "qr-token-abc123",
            generateQrSuccessExample.GetProperty("token").GetString()
        );

        var resolveQrSuccessExample = root
            .GetProperty("paths")
            .GetProperty("/api/v1/qr/{token}")
            .GetProperty("get")
            .GetProperty("responses")
            .GetProperty("200")
            .GetProperty("content")
            .GetProperty("application/json")
            .GetProperty("example");

        Assert.Equal("B01", resolveQrSuccessExample.GetProperty("tableCode").GetString());

        var createOrderViaQrSuccessExample = root
            .GetProperty("paths")
            .GetProperty("/api/v1/orders/qr")
            .GetProperty("post")
            .GetProperty("responses")
            .GetProperty("200")
            .GetProperty("content")
            .GetProperty("application/json")
            .GetProperty("example");

        Assert.Equal("Pending", createOrderViaQrSuccessExample.GetProperty("status").GetString());

        var addItemResponses = root
            .GetProperty("paths")
            .GetProperty("/api/v1/orders/{id}/items")
            .GetProperty("post")
            .GetProperty("responses");

        Assert.True(addItemResponses.TryGetProperty("204", out _));

        var closeOrderResponses = root
            .GetProperty("paths")
            .GetProperty("/api/v1/orders/{id}/close")
            .GetProperty("post")
            .GetProperty("responses");

        Assert.True(closeOrderResponses.TryGetProperty("204", out _));

        var menuResponses = root
            .GetProperty("paths")
            .GetProperty("/api/v1/tables/{tableId}/menu")
            .GetProperty("get")
            .GetProperty("responses");

        var menuSuccessExample = menuResponses
            .GetProperty("200")
            .GetProperty("content")
            .GetProperty("application/json")
            .GetProperty("example");

        Assert.Equal("M001", menuSuccessExample[0].GetProperty("code").GetString());

        var menuBadRequestExample = menuResponses
            .GetProperty("400")
            .GetProperty("content")
            .GetProperty("application/json")
            .GetProperty("example");

        Assert.Equal("TABLE_ID_REQUIRED", menuBadRequestExample.GetProperty("errorCode").GetString());

        var menuNotFoundExample = menuResponses
            .GetProperty("404")
            .GetProperty("content")
            .GetProperty("application/json")
            .GetProperty("example");

        Assert.Equal("TABLE_NOT_FOUND", menuNotFoundExample.GetProperty("errorCode").GetString());

        var healthResponses = root
            .GetProperty("paths")
            .GetProperty("/health")
            .GetProperty("get")
            .GetProperty("responses");

        Assert.True(healthResponses.TryGetProperty("500", out _));
    }

    [Fact]
    public async Task Swagger_document_should_include_examples_for_all_json_responses()
    {
        await using var factory = new TestApiFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/swagger/v1/swagger.json");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        await using var stream = await response.Content.ReadAsStreamAsync();
        using var document = await JsonDocument.ParseAsync(stream);
        var paths = document.RootElement.GetProperty("paths");
        foreach (var path in paths.EnumerateObject())
        {
            if (
                !path.Name.StartsWith("/api/v1/", StringComparison.Ordinal)
                && path.Name != "/health"
                && path.Name != "/health/live"
                && path.Name != "/health/ready"
            )
                continue;

            foreach (var operation in path.Value.EnumerateObject())
            {
                if (!operation.Value.TryGetProperty("responses", out var responses))
                    continue;

                foreach (var responseNode in responses.EnumerateObject())
                {
                    if (!responseNode.Value.TryGetProperty("content", out var content))
                        continue;

                    if (!content.TryGetProperty("application/json", out var jsonContent))
                        continue;

                    Assert.True(
                        jsonContent.TryGetProperty("example", out _),
                        $"Missing Swagger example for {operation.Name.ToUpperInvariant()} {path.Name} {responseNode.Name}"
                    );
                }
            }
        }
    }
}
