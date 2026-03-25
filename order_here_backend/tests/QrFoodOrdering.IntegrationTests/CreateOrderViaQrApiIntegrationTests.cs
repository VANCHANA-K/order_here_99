using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using QrFoodOrdering.Api.Contracts.Common;
using QrFoodOrdering.Api.Contracts.Orders;
using QrFoodOrdering.Api.Contracts.Tables;
using QrFoodOrdering.Application.Common.Errors;
using QrFoodOrdering.IntegrationTests.Infrastructure;

namespace QrFoodOrdering.IntegrationTests;

public sealed class CreateOrderViaQrApiIntegrationTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task Create_order_via_qr_should_create_pending_order()
    {
        await using var factory = new TestApiFactory();
        using var client = factory.CreateClient();

        var tableId = await CreateTableAsync(client, "QR-A01");
        var menuItems = await factory.SeedMenuItemsAsync(
            ("M001", "Pad Thai", 60m),
            ("M002", "Thai Tea", 25m)
        );

        var response = await client.PostAsJsonAsync(
            "/api/v1/orders/qr",
            new CreateOrderViaQrRequest(
                tableId,
                new List<CreateOrderViaQrItemRequest>
                {
                    new(menuItems[0].Id, 2),
                    new(menuItems[1].Id, 1)
                },
                null
            )
        );

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<CreateOrderViaQrResponse>(JsonOptions);
        Assert.NotNull(body);
        Assert.Equal("Pending", body.Status);

        var order = await client.GetFromJsonAsync<OrderResponse>(
            $"/api/v1/orders/{body.OrderId}",
            JsonOptions
        );

        Assert.NotNull(order);
        Assert.Equal("Pending", order.Status);
        Assert.Equal(145m, order.TotalAmount);
    }

    [Fact]
    public async Task Create_order_via_qr_should_be_idempotent_when_key_is_reused()
    {
        await using var factory = new TestApiFactory();
        using var client = factory.CreateClient();

        var tableId = await CreateTableAsync(client, "QR-A02");
        var menuItems = await factory.SeedMenuItemsAsync(("M003", "Fried Rice", 55m));

        var payload = new CreateOrderViaQrRequest(
            tableId,
            new List<CreateOrderViaQrItemRequest>
            {
                new(menuItems[0].Id, 3)
            },
            "qr-order-key"
        );

        var first = await client.PostAsJsonAsync("/api/v1/orders/qr", payload);
        var second = await client.PostAsJsonAsync("/api/v1/orders/qr", payload);

        var firstBody = await first.Content.ReadFromJsonAsync<CreateOrderViaQrResponse>(JsonOptions);
        var secondBody = await second.Content.ReadFromJsonAsync<CreateOrderViaQrResponse>(JsonOptions);

        Assert.NotNull(firstBody);
        Assert.NotNull(secondBody);
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(HttpStatusCode.OK, second.StatusCode);
        Assert.Equal(firstBody.OrderId, secondBody.OrderId);
    }

    [Fact]
    public async Task Create_order_via_qr_with_unavailable_item_should_return_bad_request()
    {
        await using var factory = new TestApiFactory();
        using var client = factory.CreateClient();

        var tableId = await CreateTableAsync(client, "QR-A03");
        var menuItems = await factory.SeedMenuItemsAsync(("M004", "Sold Out Item", 70m));

        await factory.ExecuteDbContextAsync(async db =>
        {
            var item = await db.MenuItems.FirstAsync(x => x.Id == menuItems[0].Id);
            item.SetAvailability(false);
            await db.SaveChangesAsync();
        });

        var response = await client.PostAsJsonAsync(
            "/api/v1/orders/qr",
            new CreateOrderViaQrRequest(
                tableId,
                new List<CreateOrderViaQrItemRequest> { new(menuItems[0].Id, 1) },
                null
            )
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(JsonOptions);
        Assert.NotNull(body);
        Assert.Equal(ApplicationErrorCodes.ItemUnavailable, body.ErrorCode);
    }

    [Fact]
    public async Task Create_order_via_qr_with_empty_items_should_return_specific_model_binding_error()
    {
        await using var factory = new TestApiFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/v1/orders/qr",
            new CreateOrderViaQrRequest(Guid.NewGuid(), [], null)
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(JsonOptions);
        Assert.NotNull(body);
        Assert.Equal(ApplicationErrorCodes.EmptyItems, body.ErrorCode);
        Assert.Equal("At least one item is required.", body.Message);
    }

    [Fact]
    public async Task Create_order_via_qr_with_invalid_quantity_should_return_specific_model_binding_error()
    {
        await using var factory = new TestApiFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/v1/orders/qr",
            new CreateOrderViaQrRequest(
                Guid.NewGuid(),
                [new CreateOrderViaQrItemRequest(Guid.NewGuid(), 0)],
                null
            )
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(JsonOptions);
        Assert.NotNull(body);
        Assert.Equal(ApplicationErrorCodes.InvalidQuantity, body.ErrorCode);
        Assert.Equal("Quantity must be greater than 0.", body.Message);
    }

    [Fact]
    public async Task Create_order_via_qr_with_empty_menu_item_id_should_return_specific_model_binding_error()
    {
        await using var factory = new TestApiFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/v1/orders/qr",
            new CreateOrderViaQrRequest(
                Guid.NewGuid(),
                [new CreateOrderViaQrItemRequest(Guid.Empty, 1)],
                null
            )
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(JsonOptions);
        Assert.NotNull(body);
        Assert.Equal(ApplicationErrorCodes.MenuItemIdRequired, body.ErrorCode);
        Assert.Equal("MenuItemId is required.", body.Message);
    }

    [Fact]
    public async Task Create_order_via_qr_with_invalid_menu_item_id_type_should_return_specific_invalid_guid_error()
    {
        await using var factory = new TestApiFactory();
        using var client = factory.CreateClient();

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/orders/qr")
        {
            Content = new StringContent(
                "{\"tableId\":\"11111111-1111-1111-1111-111111111111\",\"items\":[{\"menuItemId\":\"bad-guid\",\"quantity\":1}]}",
                System.Text.Encoding.UTF8,
                "application/json"
            )
        };

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(JsonOptions);
        Assert.NotNull(body);
        Assert.Equal(ApplicationErrorCodes.MenuItemIdInvalid, body.ErrorCode);
        Assert.Equal("MenuItemId must be a valid GUID.", body.Message);
    }

    [Fact]
    public async Task Create_order_via_qr_with_items_object_should_return_invalid_json()
    {
        await using var factory = new TestApiFactory();
        using var client = factory.CreateClient();

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/orders/qr")
        {
            Content = new StringContent(
                "{\"tableId\":\"11111111-1111-1111-1111-111111111111\",\"items\":{\"menuItemId\":\"11111111-1111-1111-1111-111111111111\",\"quantity\":1}}",
                System.Text.Encoding.UTF8,
                "application/json"
            )
        };

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(JsonOptions);
        Assert.NotNull(body);
        Assert.Equal(ApplicationErrorCodes.InvalidJson, body.ErrorCode);
        Assert.Equal("Request body contains invalid JSON.", body.Message);
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
