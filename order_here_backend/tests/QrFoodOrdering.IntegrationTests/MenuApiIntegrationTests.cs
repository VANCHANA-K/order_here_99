using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using QrFoodOrdering.Api.Contracts.Common;
using QrFoodOrdering.Api.Contracts.Menu;
using QrFoodOrdering.Api.Contracts.Tables;
using QrFoodOrdering.Application.Common.Errors;
using QrFoodOrdering.IntegrationTests.Infrastructure;

namespace QrFoodOrdering.IntegrationTests;

public sealed class MenuApiIntegrationTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task Get_menu_for_existing_table_should_return_only_active_items()
    {
        await using var factory = new TestApiFactory();
        using var client = factory.CreateClient();

        var tableId = await CreateTableAsync(client, "MENU-A01");
        var items = await factory.SeedMenuItemsAsync(
            ("M010", "Pad Thai", 60m),
            ("M011", "Thai Tea", 25m)
        );

        await factory.ExecuteDbContextAsync(async db =>
        {
            var inactive = await db.MenuItems.FirstAsync(x => x.Id == items[1].Id);
            inactive.Deactivate();
            await db.SaveChangesAsync();
        });

        var response = await client.GetAsync($"/api/v1/tables/{tableId}/menu");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<List<MenuItemResponse>>(JsonOptions);
        Assert.NotNull(body);
        var item = Assert.Single(body);
        Assert.Equal("M010", item.Code);
    }

    [Fact]
    public async Task Get_menu_with_empty_table_id_should_return_specific_bad_request()
    {
        await using var factory = new TestApiFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync($"/api/v1/tables/{Guid.Empty}/menu");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(JsonOptions);
        Assert.NotNull(body);
        Assert.Equal(ApplicationErrorCodes.TableIdRequired, body.ErrorCode);
        Assert.Equal("TableId is required.", body.Message);
    }

    [Fact]
    public async Task Get_menu_for_unknown_table_should_return_specific_not_found()
    {
        await using var factory = new TestApiFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync($"/api/v1/tables/{Guid.NewGuid()}/menu");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(JsonOptions);
        Assert.NotNull(body);
        Assert.Equal(ApplicationErrorCodes.TableNotFound, body.ErrorCode);
        Assert.Equal("Table not found.", body.Message);
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
