using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using QrFoodOrdering.Api.Contracts.Common;
using QrFoodOrdering.Api.Contracts.Orders;
using QrFoodOrdering.Api.Contracts.Tables;
using QrFoodOrdering.Application.Common.Errors;
using QrFoodOrdering.IntegrationTests.Infrastructure;

namespace QrFoodOrdering.IntegrationTests;

public sealed class OrdersApiIntegrationTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task Create_get_add_item_and_close_order_should_work_end_to_end()
    {
        await using var factory = new TestApiFactory();
        using var client = factory.CreateClient();
        var tableId = await CreateTableAsync(client, "ORD-A01");

        var createResponse = await client.PostAsJsonAsync(
            "/api/v1/orders",
            new CreateOrderRequest(tableId)
        );

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created = await createResponse.Content.ReadFromJsonAsync<CreateOrderResponse>(JsonOptions);
        Assert.NotNull(created);
        Assert.NotEqual(Guid.Empty, created.OrderId);
        Assert.Equal($"/api/v1/orders/{created.OrderId}", createResponse.Headers.Location?.AbsolutePath);

        var getResponse = await client.GetAsync($"/api/v1/orders/{created.OrderId}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var beforeAdd = await getResponse.Content.ReadFromJsonAsync<OrderResponse>(JsonOptions);
        Assert.NotNull(beforeAdd);
        Assert.Equal("Pending", beforeAdd.Status);
        Assert.Equal(0m, beforeAdd.TotalAmount);

        var addResponse = await client.PostAsJsonAsync(
            $"/api/v1/orders/{created.OrderId}/items",
            new AddItemRequest
            {
                ProductName = "Pad Thai",
                Quantity = 2,
                UnitPrice = 60m
            }
        );

        Assert.Equal(HttpStatusCode.NoContent, addResponse.StatusCode);

        var afterAddResponse = await client.GetAsync($"/api/v1/orders/{created.OrderId}");
        var afterAdd = await afterAddResponse.Content.ReadFromJsonAsync<OrderResponse>(JsonOptions);
        Assert.NotNull(afterAdd);
        Assert.Equal("Pending", afterAdd.Status);
        Assert.Equal(120m, afterAdd.TotalAmount);

        var closeResponse = await client.PostAsync($"/api/v1/orders/{created.OrderId}/close", null);
        Assert.Equal(HttpStatusCode.NoContent, closeResponse.StatusCode);

        var afterCloseResponse = await client.GetAsync($"/api/v1/orders/{created.OrderId}");
        var afterClose = await afterCloseResponse.Content.ReadFromJsonAsync<OrderResponse>(JsonOptions);
        Assert.NotNull(afterClose);
        Assert.Equal("Completed", afterClose.Status);
        Assert.Equal(120m, afterClose.TotalAmount);
    }

    [Fact]
    public async Task Create_order_should_be_idempotent_when_header_is_reused()
    {
        await using var factory = new TestApiFactory();
        using var client = factory.CreateClient();
        var tableId = await CreateTableAsync(client, "ORD-A02");

        using var first = new HttpRequestMessage(HttpMethod.Post, "/api/v1/orders")
        {
            Content = JsonContent.Create(new CreateOrderRequest(tableId))
        };
        first.Headers.Add("Idempotency-Key", "same-key");

        using var second = new HttpRequestMessage(HttpMethod.Post, "/api/v1/orders")
        {
            Content = JsonContent.Create(new CreateOrderRequest(tableId))
        };
        second.Headers.Add("Idempotency-Key", "same-key");

        var firstResponse = await client.SendAsync(first);
        var secondResponse = await client.SendAsync(second);

        var firstBody = await firstResponse.Content.ReadFromJsonAsync<CreateOrderResponse>(JsonOptions);
        var secondBody = await secondResponse.Content.ReadFromJsonAsync<CreateOrderResponse>(JsonOptions);

        Assert.NotNull(firstBody);
        Assert.NotNull(secondBody);
        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Created, secondResponse.StatusCode);
        Assert.Equal(firstBody.OrderId, secondBody.OrderId);
    }

    [Fact]
    public async Task Add_item_with_invalid_payload_should_return_specific_error_code()
    {
        await using var factory = new TestApiFactory();
        using var client = factory.CreateClient();
        var tableId = await CreateTableAsync(client, "ORD-A03");

        var createResponse = await client.PostAsJsonAsync("/api/v1/orders", new CreateOrderRequest(tableId));
        var created = await createResponse.Content.ReadFromJsonAsync<CreateOrderResponse>(JsonOptions);
        Assert.NotNull(created);

        var response = await client.PostAsJsonAsync(
            $"/api/v1/orders/{created.OrderId}/items",
            new AddItemRequest
            {
                ProductName = "",
                Quantity = 0,
                UnitPrice = 0m
            }
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(JsonOptions);
        Assert.NotNull(body);
        Assert.Equal(ApplicationErrorCodes.ProductNameRequired, body.ErrorCode);
        Assert.Equal("ProductName is required.", body.Message);
        Assert.False(string.IsNullOrWhiteSpace(body.TraceId));
    }

    [Fact]
    public async Task Create_order_with_empty_table_id_should_return_specific_validation_error()
    {
        await using var factory = new TestApiFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/orders", new CreateOrderRequest(Guid.Empty));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(JsonOptions);
        Assert.NotNull(body);
        Assert.Equal(ApplicationErrorCodes.TableIdRequired, body.ErrorCode);
        Assert.Equal("TableId is required.", body.Message);
    }

    [Fact]
    public async Task Create_order_with_missing_body_should_return_request_body_required()
    {
        await using var factory = new TestApiFactory();
        using var client = factory.CreateClient();

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/orders")
        {
            Content = new StringContent(string.Empty, Encoding.UTF8, "application/json")
        };

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(JsonOptions);
        Assert.NotNull(body);
        Assert.Equal(ApplicationErrorCodes.RequestBodyRequired, body.ErrorCode);
        Assert.Equal("Request body is required.", body.Message);
    }

    [Fact]
    public async Task Create_order_with_invalid_json_should_return_invalid_json()
    {
        await using var factory = new TestApiFactory();
        using var client = factory.CreateClient();

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/orders")
        {
            Content = new StringContent("{", Encoding.UTF8, "application/json")
        };

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(JsonOptions);
        Assert.NotNull(body);
        Assert.Equal(ApplicationErrorCodes.InvalidJson, body.ErrorCode);
        Assert.Equal("Request body contains invalid JSON.", body.Message);
    }

    [Fact]
    public async Task Create_order_with_invalid_table_id_type_should_return_table_id_invalid()
    {
        await using var factory = new TestApiFactory();
        using var client = factory.CreateClient();

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/orders")
        {
            Content = new StringContent("{\"tableId\":\"not-a-guid\"}", Encoding.UTF8, "application/json")
        };

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(JsonOptions);
        Assert.NotNull(body);
        Assert.Equal(ApplicationErrorCodes.TableIdInvalid, body.ErrorCode);
        Assert.Equal("TableId must be a valid GUID.", body.Message);
    }

    [Fact]
    public async Task Add_item_with_quantity_string_should_return_invalid_quantity()
    {
        await using var factory = new TestApiFactory();
        using var client = factory.CreateClient();

        using var request = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/orders/{Guid.NewGuid()}/items")
        {
            Content = new StringContent(
                "{\"productName\":\"Pad Thai\",\"quantity\":\"two\",\"unitPrice\":60}",
                Encoding.UTF8,
                "application/json"
            )
        };

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(JsonOptions);
        Assert.NotNull(body);
        Assert.Equal(ApplicationErrorCodes.InvalidQuantity, body.ErrorCode);
        Assert.Equal("Quantity must be greater than 0.", body.Message);
    }

    [Fact]
    public async Task Add_item_should_be_idempotent_when_header_is_reused()
    {
        await using var factory = new TestApiFactory();
        using var client = factory.CreateClient();
        var tableId = await CreateTableAsync(client, "ORD-A04");

        var createResponse = await client.PostAsJsonAsync(
            "/api/v1/orders",
            new CreateOrderRequest(tableId)
        );
        var created = await createResponse.Content.ReadFromJsonAsync<CreateOrderResponse>(JsonOptions);
        Assert.NotNull(created);

        using var first = new HttpRequestMessage(
            HttpMethod.Post,
            $"/api/v1/orders/{created.OrderId}/items"
        )
        {
            Content = JsonContent.Create(new AddItemRequest
            {
                ProductName = "Pad Thai",
                Quantity = 2,
                UnitPrice = 60m
            })
        };
        first.Headers.Add("Idempotency-Key", "same-add-key");

        using var second = new HttpRequestMessage(
            HttpMethod.Post,
            $"/api/v1/orders/{created.OrderId}/items"
        )
        {
            Content = JsonContent.Create(new AddItemRequest
            {
                ProductName = "Pad Thai",
                Quantity = 2,
                UnitPrice = 60m
            })
        };
        second.Headers.Add("Idempotency-Key", "same-add-key");

        Assert.Equal(HttpStatusCode.NoContent, (await client.SendAsync(first)).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await client.SendAsync(second)).StatusCode);

        var order = await client.GetFromJsonAsync<OrderResponse>(
            $"/api/v1/orders/{created.OrderId}",
            JsonOptions
        );

        Assert.NotNull(order);
        Assert.Equal(120m, order.TotalAmount);
    }

    [Fact]
    public async Task Close_missing_order_should_return_not_found_error_shape()
    {
        await using var factory = new TestApiFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsync($"/api/v1/orders/{Guid.NewGuid()}/close", null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(JsonOptions);
        Assert.NotNull(body);
        Assert.Equal(ApplicationErrorCodes.OrderNotFound, body.ErrorCode);
        Assert.Equal("Order not found", body.Message);
    }

    [Fact]
    public async Task Unmatched_route_should_return_endpoint_not_found_error_shape()
    {
        await using var factory = new TestApiFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/v1/does-not-exist");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(JsonOptions);
        Assert.NotNull(body);
        Assert.Equal(ApiErrorCodes.EndpointNotFound, body.ErrorCode);
        Assert.Equal("The requested endpoint was not found.", body.Message);
        Assert.False(string.IsNullOrWhiteSpace(body.TraceId));
    }

    [Fact]
    public async Task Unexpected_error_should_return_generic_error_without_leaking_message()
    {
        await using var factory = new TestApiFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/test/error");

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(JsonOptions);
        Assert.NotNull(body);
        Assert.Equal(ApiErrorCodes.UnexpectedError, body.ErrorCode);
        Assert.Equal("Unexpected error occurred.", body.Message);
        Assert.DoesNotContain("THIS_MESSAGE_MUST_NOT_LEAK", body.Message, StringComparison.Ordinal);
        Assert.False(string.IsNullOrWhiteSpace(body.TraceId));
    }

    [Fact]
    public async Task Create_order_with_unknown_table_should_return_table_not_found()
    {
        await using var factory = new TestApiFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/v1/orders",
            new CreateOrderRequest(Guid.NewGuid())
        );

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(JsonOptions);
        Assert.NotNull(body);
        Assert.Equal(ApplicationErrorCodes.TableNotFound, body.ErrorCode);
        Assert.Equal("Table not found.", body.Message);
    }

    [Fact]
    public async Task Create_order_with_inactive_table_should_return_conflict()
    {
        await using var factory = new TestApiFactory();
        using var client = factory.CreateClient();

        var tableId = await CreateTableAsync(client, "ORD-A05");
        var disableResponse = await client.PatchAsync($"/api/v1/tables/{tableId}/disable", null);
        Assert.Equal(HttpStatusCode.NoContent, disableResponse.StatusCode);

        var response = await client.PostAsJsonAsync(
            "/api/v1/orders",
            new CreateOrderRequest(tableId)
        );

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(JsonOptions);
        Assert.NotNull(body);
        Assert.Equal(ApplicationErrorCodes.TableInactive, body.ErrorCode);
        Assert.Equal("Table is inactive.", body.Message);
    }

    private static async Task<Guid> CreateTableAsync(HttpClient client, string code)
    {
        var response = await client.PostAsJsonAsync("/api/v1/tables", new CreateTableRequest(code));
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<CreatedIdResponse>(JsonOptions);
        Assert.NotNull(body);
        return body.Id;
    }
}
