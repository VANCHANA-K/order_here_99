# Repository Guidelines

## Project Structure & Module Organization
- Solution: `QrFoodOrdering.sln`
- Source: `src/`
  - `QrFoodOrdering.Api` (ASP.NET Core API, entrypoint)
  - `QrFoodOrdering.Application` (use cases, handlers, policies)
  - `QrFoodOrdering.Domain` (entities, value objects, rules)
  - `QrFoodOrdering.Infrastructure` (EF Core, repositories, migrations)
- Tests: `tests/`
  - `QrFoodOrdering.UnitTests`
  - `QrFoodOrdering.IntegrationTests`
- Config: `src/QrFoodOrdering.Api/appsettings.*.json`

## Build, Test, and Development Commands
- Restore: `dotnet restore QrFoodOrdering.sln`
- Build: `dotnet build QrFoodOrdering.sln`
- Run API (dev): `dotnet run --project src/QrFoodOrdering.Api/QrFoodOrdering.Api.csproj`
- Watch run: `dotnet watch --project src/QrFoodOrdering.Api/QrFoodOrdering.Api.csproj run`
- Tests: `dotnet test QrFoodOrdering.sln`
- Format (SDK): `dotnet format` (run at repo root)

## Coding Style & Naming Conventions
- C# with nullable enabled (`<Nullable>enable</Nullable>` in projects).
- Indentation: 4 spaces; braces on new lines (C# style).
- Naming: `PascalCase` for types/members, `camelCase` for locals/params, interfaces prefixed with `I` (e.g., `IOrderRepository`).
- Async methods end with `Async` where applicable; cancellation via `CancellationToken`.
- Folders align with namespaces; API request/response DTOs live under `Api/Contracts`.

## Testing Guidelines
- Framework: xUnit.
- Location: under `tests/`; mirror source namespaces when possible.
- Naming: `MethodOrBehavior_State_ExpectedOutcome` (see `CloseOrderHandlerUnitTests`).
- Scope: prefer unit tests in Domain/Application; avoid hitting real SQLite in unit tests.
- Run: `dotnet test QrFoodOrdering.sln`; ensure all tests pass before PR.

## Request Validation Rule
- Every new API request DTO must follow the same validation pattern used by existing DTOs.
- Required checklist for any new request DTO:
  - add explicit data annotations on the DTO in `src/QrFoodOrdering.Api/Contracts`
  - add or update a specific mapping in `src/QrFoodOrdering.Api/Validation/ModelValidationErrorMapper.cs`
  - add at least one integration test for an invalid payload in `tests/QrFoodOrdering.IntegrationTests`
- Prefer specific error codes such as `TABLE_ID_REQUIRED` or `INVALID_QTY`; do not rely on `INVALID_REQUEST` unless no specific mapping is possible.
- If a field can fail in more than one way, keep the semantics split:
  - missing/empty -> `*_REQUIRED`
  - malformed type/JSON conversion -> `*_INVALID` or `INVALID_JSON`

## Commit & Pull Request Guidelines
- Use Conventional Commits where possible: `feat:`, `fix:`, `chore:`, `ci:`, `docs:`; include scope (e.g., `feat(api): create table`).
- PRs must include: summary, linked issues, testing steps (e.g., `curl http://localhost:5132/health`), and screenshots/logs if relevant.
- CI (restore/build/test) must be green.

## Security & Configuration Tips
- Do not commit secrets; prefer env vars or User Secrets. SQLite files and logs are gitignored.
- Environments: set `ASPNETCORE_ENVIRONMENT` (`Development`, `Test`, `Production`).
- Runtime behavior is controlled by `Runtime` in `src/QrFoodOrdering.Api/appsettings.*.json`:
  - `Development`: Swagger/CORS/migrate/seed enabled
  - `Test`: Swagger enabled, migrate/seed disabled
  - `Production`: Swagger/CORS/migrate/seed disabled
- Development can use `dotnet user-secrets` for local secret overrides.
- Production must provide `ConnectionStrings__Default` via environment variable or secret store; do not add production secrets to `appsettings.*.json`.
- Default dev DB fallback remains in `appsettings.Development.json` for local SQLite only.

## Architecture Overview
- Clean Architecture: API → Application → Domain; Infrastructure referenced by Application for implementations.
- Keep Domain free of external dependencies; place EF/migrations in `Infrastructure/Migrations`.
