# Order Here – Backend

A QR food ordering backend built with:

- ASP.NET Core (.NET 9)
- Clean Architecture (Domain / Application / Infrastructure / API)
- SQLite (Development)
- xUnit (Testing)
- GitHub Actions CI

- .NET SDK 9.x
- Git

## Quick Start

From `order_here_backend`:

```bash
dotnet restore QrFoodOrdering.sln
dotnet build QrFoodOrdering.sln
dotnet run --project src/QrFoodOrdering.Api/QrFoodOrdering.Api.csproj
```

API runs at `http://localhost:5132`.

## Tests

```bash
dotnet test QrFoodOrdering.sln
```

Run test projects separately when needed:

```bash
dotnet test tests/QrFoodOrdering.UnitTests/QrFoodOrdering.UnitTests.csproj
dotnet test tests/QrFoodOrdering.IntegrationTests/QrFoodOrdering.IntegrationTests.csproj
```

## Health Check

```bash
curl http://localhost:5132/health
```

Expected response:

```json
{
  "status": "ok"
}
```

Response includes:

- HTTP 200
- `x-trace-id` header
- JSON payload

## API Error Semantics

Detailed API error policy lives in [`docs/API_ERROR_SEMANTICS.md`](/Users/viic/Desktop/order_here/order_here_backend/docs/API_ERROR_SEMANTICS.md).

In short:

- all non-success responses use `ApiErrorResponse`
- `400/404/409/500` have fixed semantics
- resource not found uses specific codes such as `ORDER_NOT_FOUND`
- `INVALID_REQUEST` is reserved as a final fallback only
- Swagger publishes both error and success examples for documented endpoints
- every new request DTO must include annotations, a `ModelValidationErrorMapper` rule, and an integration test for invalid payloads

## Architecture

Backend follows Clean Architecture:

- Domain → Business rules
- Application → Use cases
- Infrastructure → EF Core + SQLite
- API → HTTP endpoints
- Tests → Domain & Application validation

Dependency flow:

```
API → Application → Domain
Infrastructure → Application
```

Domain layer has zero external dependencies.

## CI Pipeline

GitHub Actions runs:

- dotnet restore
- dotnet format --verify-no-changes
- dotnet build
- dotnet test (`UnitTests`)
- dotnet test (`IntegrationTests`)

Target solution:

```
order_here_backend/QrFoodOrdering.sln
```

CI must pass before merge to main.

## Environment

- Development (default)
- Test (CI)

No secrets stored in repository.
SQLite is used for development only.
