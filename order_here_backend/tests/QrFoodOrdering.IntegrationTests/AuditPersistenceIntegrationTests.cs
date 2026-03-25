using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using QrFoodOrdering.Api.Contracts.Common;
using QrFoodOrdering.Api.Contracts.Orders;
using QrFoodOrdering.Api.Contracts.Qr;
using QrFoodOrdering.Api.Contracts.Tables;
using QrFoodOrdering.Application.Common.Audit;
using QrFoodOrdering.Domain.Audit;
using QrFoodOrdering.IntegrationTests.Infrastructure;

namespace QrFoodOrdering.IntegrationTests;

public sealed class AuditPersistenceIntegrationTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task Health_endpoint_should_not_depend_on_audit_writer()
    {
        await using var factory = new TestApiFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<HealthResponse>(JsonOptions);
        Assert.NotNull(payload);
        Assert.Equal("ok", payload.Status);
        Assert.Empty(factory.WriterAuditLogs);
    }

    [Fact]
    public async Task Create_and_disable_table_should_write_audit_logs_via_writer()
    {
        await using var factory = new TestApiFactory();
        using var client = factory.CreateClient();

        var createResponse = await client.PostAsJsonAsync(
            "/api/v1/tables",
            new CreateTableRequest("AUDIT-T1")
        );
        createResponse.EnsureSuccessStatusCode();

        var created = await createResponse.Content.ReadFromJsonAsync<CreatedIdResponse>(JsonOptions);
        Assert.NotNull(created);

        var disableResponse = await client.PatchAsync($"/api/v1/tables/{created.Id}/disable", null);
        Assert.Equal(HttpStatusCode.NoContent, disableResponse.StatusCode);

        Assert.Contains(
            factory.WriterAuditLogs,
            x => x.EventType == AuditEvents.TableCreated
                && x.EntityType == AuditEntities.Table
                && x.EntityId == created.Id
        );

        Assert.Contains(
            factory.WriterAuditLogs,
            x => x.EventType == AuditEvents.TableStatusChanged
                && x.EntityType == AuditEntities.Table
                && x.EntityId == created.Id
        );
    }

    [Fact]
    public async Task Generate_and_resolve_qr_should_persist_audit_logs_in_database()
    {
        await using var factory = new TestApiFactory();
        using var client = factory.CreateClient();

        var tableId = await CreateTableAsync(client, "AUDIT-T2");

        var generateResponse = await client.PostAsync($"/api/v1/tables/{tableId}/qr", null);
        generateResponse.EnsureSuccessStatusCode();

        var generated = await generateResponse.Content.ReadFromJsonAsync<GenerateQrResponse>(JsonOptions);
        Assert.NotNull(generated);

        var resolveResponse = await client.GetAsync($"/api/v1/qr/{generated.Token}");
        resolveResponse.EnsureSuccessStatusCode();

        var logs = await factory.ExecuteDbContextAsync(db =>
            Task.FromResult(db.AuditLogs.ToList())
        );

        Assert.Contains(
            logs,
            x => x.EventType == AuditEvents.QrGenerated
                && x.EntityType == AuditEntities.Table
                && x.EntityId == tableId
                && x.Metadata == generated.Token
        );

        Assert.Contains(
            logs,
            x => x.EventType == AuditEvents.QrResolved
                && x.EntityType == AuditEntities.QrCode
                && x.Metadata == generated.Token
        );
    }

    [Fact]
    public async Task Create_order_via_qr_should_persist_order_audit_log_in_database()
    {
        await using var factory = new TestApiFactory();
        using var client = factory.CreateClient();

        var tableId = await CreateTableAsync(client, "AUDIT-T3");
        var menuItems = await factory.SeedMenuItemsAsync(("AUDIT-M1", "Pad Thai", 60m));

        var response = await client.PostAsJsonAsync(
            "/api/v1/orders/qr",
            new CreateOrderViaQrRequest(
                tableId,
                [new CreateOrderViaQrItemRequest(menuItems[0].Id, 2)],
                null
            )
        );
        response.EnsureSuccessStatusCode();

        var created = await response.Content.ReadFromJsonAsync<CreateOrderViaQrResponse>(JsonOptions);
        Assert.NotNull(created);

        var logs = await factory.ExecuteDbContextAsync(db =>
            Task.FromResult(db.AuditLogs.ToList())
        );

        Assert.Contains(
            logs,
            x => x.EventType == AuditEvents.OrderPlacedViaQr
                && x.EntityType == AuditEntities.Order
                && x.EntityId == created.OrderId
                && x.Metadata is not null
                && x.Metadata.Contains(created.OrderId.ToString(), StringComparison.Ordinal)
        );
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
