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
    public async Task Liveness_and_readiness_endpoints_should_return_ok_when_app_is_ready()
    {
        await using var factory = new TestApiFactory();
        using var client = factory.CreateClient();

        var liveResponse = await client.GetAsync("/health/live");
        var readyResponse = await client.GetAsync("/health/ready");

        Assert.Equal(HttpStatusCode.OK, liveResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, readyResponse.StatusCode);

        var livePayload = await liveResponse.Content.ReadFromJsonAsync<HealthResponse>(JsonOptions);
        var readyPayload = await readyResponse.Content.ReadFromJsonAsync<HealthResponse>(JsonOptions);

        Assert.NotNull(livePayload);
        Assert.NotNull(readyPayload);
        Assert.Equal("ok", livePayload.Status);
        Assert.Equal("ok", readyPayload.Status);
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
                && !string.IsNullOrWhiteSpace(x.TraceId)
        );

        Assert.Contains(
            logs,
            x => x.EventType == AuditEvents.QrResolved
                && x.EntityType == AuditEntities.QrCode
                && x.Metadata == generated.Token
                && !string.IsNullOrWhiteSpace(x.TraceId)
        );
    }

    [Fact]
    public async Task Audit_logs_should_persist_trace_id_from_request_header_end_to_end()
    {
        await using var factory = new TestApiFactory();
        using var client = factory.CreateClient();

        const string traceId = "trace-e2e-audit-001";
        client.DefaultRequestHeaders.Add("X-Trace-Id", traceId);

        var tableId = await CreateTableAsync(client, "AUDIT-T-TRACE");

        using var createRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/orders")
        {
            Content = JsonContent.Create(new CreateOrderRequest(tableId))
        };
        createRequest.Headers.Add("Idempotency-Key", "audit-trace-create");

        var createResponse = await client.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();

        var created = await createResponse.Content.ReadFromJsonAsync<CreateOrderResponse>(JsonOptions);
        Assert.NotNull(created);

        var addItemResponse = await client.PostAsJsonAsync(
            $"/api/v1/orders/{created.OrderId}/items",
            new AddItemRequest
            {
                ProductName = "Trace Rice",
                Quantity = 1,
                UnitPrice = 55m
            }
        );
        Assert.Equal(HttpStatusCode.NoContent, addItemResponse.StatusCode);

        var logs = await factory.ExecuteDbContextAsync(db => Task.FromResult(db.AuditLogs.ToList()));

        Assert.Contains(
            logs,
            x => x.EntityId == created.OrderId
                && x.EventType == AuditEvents.OrderCreated
                && x.TraceId == traceId
        );

        Assert.Contains(
            logs,
            x => x.EntityId == created.OrderId
                && x.EventType == AuditEvents.OrderItemAdded
                && x.TraceId == traceId
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

    [Fact]
    public async Task Create_add_item_and_close_order_should_persist_order_audit_logs_in_database()
    {
        await using var factory = new TestApiFactory();
        using var client = factory.CreateClient();

        var tableId = await CreateTableAsync(client, "AUDIT-T4");

        using var createRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/orders")
        {
            Content = JsonContent.Create(new CreateOrderRequest(tableId))
        };
        createRequest.Headers.Add("Idempotency-Key", "audit-order-create");

        var createResponse = await client.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();

        var created = await createResponse.Content.ReadFromJsonAsync<CreateOrderResponse>(JsonOptions);
        Assert.NotNull(created);

        var addItemResponse = await client.PostAsJsonAsync(
            $"/api/v1/orders/{created.OrderId}/items",
            new AddItemRequest
            {
                ProductName = "Audit Noodles",
                Quantity = 2,
                UnitPrice = 75m
            }
        );
        Assert.Equal(HttpStatusCode.NoContent, addItemResponse.StatusCode);

        var closeResponse = await client.PostAsync($"/api/v1/orders/{created.OrderId}/close", null);
        Assert.Equal(HttpStatusCode.NoContent, closeResponse.StatusCode);

        var logs = await factory.ExecuteDbContextAsync(db => Task.FromResult(db.AuditLogs.ToList()));

        Assert.Contains(
            logs,
            x => x.EventType == AuditEvents.OrderCreated
                && x.EntityType == AuditEntities.Order
                && x.EntityId == created.OrderId
                && x.Metadata is not null
                && x.Metadata.Contains(tableId.ToString(), StringComparison.Ordinal)
        );

        Assert.Contains(
            logs,
            x => x.EventType == AuditEvents.OrderItemAdded
                && x.EntityType == AuditEntities.Order
                && x.EntityId == created.OrderId
                && x.Metadata is not null
                && x.Metadata.Contains("Audit Noodles", StringComparison.Ordinal)
        );

        Assert.Contains(
            logs,
            x => x.EventType == AuditEvents.OrderClosed
                && x.EntityType == AuditEntities.Order
                && x.EntityId == created.OrderId
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
