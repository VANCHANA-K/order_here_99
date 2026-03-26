using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using QrFoodOrdering.Api.Contracts.Common;
using QrFoodOrdering.Api.Contracts.Qr;
using QrFoodOrdering.Api.Contracts.Tables;
using QrFoodOrdering.Application.Common.Errors;
using QrFoodOrdering.Domain.Common;
using QrFoodOrdering.IntegrationTests.Infrastructure;

namespace QrFoodOrdering.IntegrationTests;

public sealed class TablesAndQrApiIntegrationTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task Create_list_disable_and_activate_table_should_work_end_to_end()
    {
        await using var factory = new TestApiFactory();
        using var client = factory.CreateClient();

        var createResponse = await client.PostAsJsonAsync(
            "/api/v1/tables",
            new CreateTableRequest("A01")
        );

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created = await createResponse.Content.ReadFromJsonAsync<CreatedIdResponse>(JsonOptions);
        Assert.NotNull(created);
        Assert.NotEqual(Guid.Empty, created.Id);

        var listResponse = await client.GetAsync("/api/v1/tables");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);

        var tables = await listResponse.Content.ReadFromJsonAsync<List<TableResponse>>(JsonOptions);
        Assert.NotNull(tables);
        var table = Assert.Single(tables);
        Assert.Equal(created.Id, table.Id);
        Assert.Equal("A01", table.Code);
        Assert.Equal("Active", table.Status);

        var disableResponse = await client.PatchAsync($"/api/v1/tables/{created.Id}/disable", null);
        Assert.Equal(HttpStatusCode.NoContent, disableResponse.StatusCode);

        var afterDisable = await client.GetFromJsonAsync<List<TableResponse>>("/api/v1/tables", JsonOptions);
        Assert.NotNull(afterDisable);
        Assert.Equal("Inactive", Assert.Single(afterDisable).Status);

        var activateResponse = await client.PostAsync($"/api/v1/tables/{created.Id}/activate", null);
        Assert.Equal(HttpStatusCode.NoContent, activateResponse.StatusCode);

        var afterActivate = await client.GetFromJsonAsync<List<TableResponse>>("/api/v1/tables", JsonOptions);
        Assert.NotNull(afterActivate);
        Assert.Equal("Active", Assert.Single(afterActivate).Status);
    }

    [Fact]
    public async Task Create_table_with_blank_code_should_return_domain_validation_error()
    {
        await using var factory = new TestApiFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/v1/tables",
            new CreateTableRequest("")
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(JsonOptions);
        Assert.NotNull(body);
        Assert.Equal(DomainErrorCodes.TableCodeRequired, body.ErrorCode);
        Assert.Equal("Table code is required", body.Message);
    }

    [Fact]
    public async Task Disable_table_twice_should_return_conflict_domain_error()
    {
        await using var factory = new TestApiFactory();
        using var client = factory.CreateClient();

        var createResponse = await client.PostAsJsonAsync(
            "/api/v1/tables",
            new CreateTableRequest("A02")
        );
        var created = await createResponse.Content.ReadFromJsonAsync<CreatedIdResponse>(JsonOptions);
        Assert.NotNull(created);

        Assert.Equal(
            HttpStatusCode.NoContent,
            (await client.PatchAsync($"/api/v1/tables/{created.Id}/disable", null)).StatusCode
        );

        var secondDisable = await client.PatchAsync($"/api/v1/tables/{created.Id}/disable", null);
        Assert.Equal(HttpStatusCode.Conflict, secondDisable.StatusCode);

        var body = await secondDisable.Content.ReadFromJsonAsync<ApiErrorResponse>(JsonOptions);
        Assert.NotNull(body);
        Assert.Equal(DomainErrorCodes.TableAlreadyInactive, body.ErrorCode);
    }

    [Fact]
    public async Task Disable_table_in_parallel_should_return_one_success_and_one_conflict()
    {
        await using var factory = new TestApiFactory();
        using var client = factory.CreateClient();

        var createResponse = await client.PostAsJsonAsync(
            "/api/v1/tables",
            new CreateTableRequest("A02-P")
        );
        var created = await createResponse.Content.ReadFromJsonAsync<CreatedIdResponse>(JsonOptions);
        Assert.NotNull(created);

        Task<HttpResponseMessage> SendDisableAsync() =>
            client.PatchAsync($"/api/v1/tables/{created.Id}/disable", null);

        var responses = await Task.WhenAll(SendDisableAsync(), SendDisableAsync());

        Assert.Contains(responses, x => x.StatusCode == HttpStatusCode.NoContent);
        Assert.Contains(responses, x => x.StatusCode == HttpStatusCode.Conflict);

        var conflictResponse = responses.Single(x => x.StatusCode == HttpStatusCode.Conflict);
        var body = await conflictResponse.Content.ReadFromJsonAsync<ApiErrorResponse>(JsonOptions);
        Assert.NotNull(body);
        Assert.Contains(
            body.ErrorCode,
            new[]
            {
                DomainErrorCodes.TableAlreadyInactive,
                ApplicationErrorCodes.ConcurrencyConflict
            }
        );

        var tables = await client.GetFromJsonAsync<List<TableResponse>>("/api/v1/tables", JsonOptions);
        Assert.NotNull(tables);
        Assert.Equal("Inactive", Assert.Single(tables).Status);
    }

    [Fact]
    public async Task Create_table_with_duplicate_code_should_return_conflict_error()
    {
        await using var factory = new TestApiFactory();
        using var client = factory.CreateClient();

        Assert.Equal(
            HttpStatusCode.Created,
            (await client.PostAsJsonAsync("/api/v1/tables", new CreateTableRequest("A03"))).StatusCode
        );

        var duplicate = await client.PostAsJsonAsync("/api/v1/tables", new CreateTableRequest("A03"));
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);

        var body = await duplicate.Content.ReadFromJsonAsync<ApiErrorResponse>(JsonOptions);
        Assert.NotNull(body);
        Assert.Equal(ApplicationErrorCodes.TableCodeAlreadyExists, body.ErrorCode);
        Assert.Equal("Table code already exists.", body.Message);
    }

    [Fact]
    public async Task Generate_qr_and_resolve_should_work_end_to_end()
    {
        await using var factory = new TestApiFactory();
        using var client = factory.CreateClient();

        var createResponse = await client.PostAsJsonAsync(
            "/api/v1/tables",
            new CreateTableRequest("B01")
        );
        var created = await createResponse.Content.ReadFromJsonAsync<CreatedIdResponse>(JsonOptions);
        Assert.NotNull(created);

        var generateResponse = await client.PostAsync($"/api/v1/tables/{created.Id}/qr", null);
        Assert.Equal(HttpStatusCode.OK, generateResponse.StatusCode);

        var generated = await generateResponse.Content.ReadFromJsonAsync<GenerateQrResponse>(JsonOptions);
        Assert.NotNull(generated);
        Assert.Equal(created.Id, generated.TableId);
        Assert.False(string.IsNullOrWhiteSpace(generated.Token));
        Assert.Contains(generated.Token, generated.QrUrl, StringComparison.Ordinal);

        var resolveResponse = await client.GetAsync($"/api/v1/qr/{generated.Token}");
        Assert.Equal(HttpStatusCode.OK, resolveResponse.StatusCode);

        var resolved = await resolveResponse.Content.ReadFromJsonAsync<ResolveQrResponse>(JsonOptions);
        Assert.NotNull(resolved);
        Assert.Equal(created.Id, resolved.TableId);
        Assert.Equal("B01", resolved.TableCode);
    }

    [Fact]
    public async Task Generate_qr_for_inactive_table_should_return_conflict()
    {
        await using var factory = new TestApiFactory();
        using var client = factory.CreateClient();

        var createResponse = await client.PostAsJsonAsync(
            "/api/v1/tables",
            new CreateTableRequest("B02")
        );
        var created = await createResponse.Content.ReadFromJsonAsync<CreatedIdResponse>(JsonOptions);
        Assert.NotNull(created);

        await client.PatchAsync($"/api/v1/tables/{created.Id}/disable", null);

        var response = await client.PostAsync($"/api/v1/tables/{created.Id}/qr", null);
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(JsonOptions);
        Assert.NotNull(body);
        Assert.Equal(ApplicationErrorCodes.TableInactive, body.ErrorCode);
    }

    [Fact]
    public async Task Resolve_unknown_qr_should_return_not_found_shape()
    {
        await using var factory = new TestApiFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync($"/api/v1/qr/{Guid.NewGuid():N}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(JsonOptions);
        Assert.NotNull(body);
        Assert.Equal(ApplicationErrorCodes.QrNotFound, body.ErrorCode);
    }

}
