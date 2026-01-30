# Repository Guidelines

## Project Structure & Module Organization
- `src/`
  - `QrFoodOrdering.Api/` — ASP.NET Core API (controllers, middleware, contracts, appsettings, data/).
  - `QrFoodOrdering.Application/` — use cases, DI, repository abstractions.
  - `QrFoodOrdering.Domain/` — entities, value objects, domain rules/exceptions.
  - `QrFoodOrdering.Infrastructure/` — EF Core, DbContext, repositories, migrations.
- `tests/`
  - `QrFoodOrdering.Tests/`, `QrFoodOrdering.Domain.Tests/` — xUnit unit tests.
- Solution: `QrFoodOrdering.sln`.

## Build, Test, and Development Commands
- Restore/build: `dotnet restore QrFoodOrdering.sln` then `dotnet build QrFoodOrdering.sln`.
- Run API: `dotnet run --project src/QrFoodOrdering.Api/QrFoodOrdering.Api.csproj`.
- Test all: `dotnet test QrFoodOrdering.sln`.
- Migrations (optional, EF tools required): `dotnet ef database update --project src/QrFoodOrdering.Infrastructure`.
- Target framework: `net9.0` (use .NET SDK 9.x).

## Coding Style & Naming Conventions
- C# with 4‑space indentation; braces on new line (Allman).
- Types/methods/properties: PascalCase. Locals/params: camelCase. Private fields: `_camelCase`.
- File names match primary type (e.g., `Order.cs`, `OrdersController.cs`).
- Keep domain logic in `Domain`, orchestration in `Application`, I/O in `Infrastructure`, endpoints in `Api`.

## Testing Guidelines
- Framework: xUnit. Place tests under `tests/` mirroring source namespaces.
- Name tests descriptively (e.g., `Create_order_should_start_as_created`).
- Run: `dotnet test`; filter with `--filter FullyQualifiedName~OrderTests` when needed.

## Commit & Pull Request Guidelines
- Prefer Conventional Commits (`feat:`, `fix:`, `chore:`, `docs:`, `test:`). Keep subjects imperative and concise.
- PRs to `main` must build and pass tests (see `.github/workflows/ci.yml`).
- Include: purpose, scope, linked issues, and API examples if endpoints change (update `src/QrFoodOrdering.Api/QrFoodOrdering.Api.http` when relevant).

## Security & Configuration Tips
- Do not commit new local DBs/logs under `src/QrFoodOrdering.Api/data/`.
- Configure connection string via `ConnectionStrings:Default` or env var `ConnectionStrings__Default`.
- No secrets in repo; use per‑env `appsettings.{Environment}.json` or environment variables.
