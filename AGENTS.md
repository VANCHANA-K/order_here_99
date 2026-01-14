# Repository Guidelines

## Project Structure & Modules
- `QrFoodOrdering.sln` — solution entry.
- `src/QrFoodOrdering.Api` — ASP.NET Core Web API (net9.0).
- `src/QrFoodOrdering.Application` — use cases (orchestrates domain logic).
- `src/QrFoodOrdering.Domain` — core entities and rules only.
- `src/QrFoodOrdering.Infrastructure` — persistence/integrations (thin for now).
- `tests/QrFoodOrdering.Tests` — xUnit tests.

## Build, Test, Run
- Restore: `dotnet restore`
- Build: `dotnet build -c Debug` (or `Release`).
- Test: `dotnet test` (optional coverage: `dotnet test --collect:"XPlat Code Coverage"`).
- Run API: `dotnet run --project src/QrFoodOrdering.Api`
- Hot reload: `dotnet watch --project src/QrFoodOrdering.Api run`

## Coding Style & Naming
- C#/.NET: net9.0, file-scoped namespaces; 4-space indentation.
- Naming: PascalCase for types/methods; camelCase for locals/params; `_camelCase` for private fields.
- One public type per file; file name matches type (e.g., `Order.cs`).
- Keep `Domain` free of framework dependencies; inject via `Application`.
- Format and lint: `dotnet format` (run before PRs).

## Testing Guidelines
- Framework: xUnit in `tests/QrFoodOrdering.Tests`.
- File naming: `*Tests.cs`; class naming: `<TypeUnderTest>Tests`.
- Method naming: `MethodName_Should_ExpectedBehavior`.
- Focus: unit tests for `Domain` and `Application`; thin API tests acceptable.
- Run locally with `dotnet test`; add assertions for edge cases and invalid states.

## Commit & Pull Requests
- Commits: small, focused, imperative subject (e.g., "Add CreateOrder use case").
- Include scope in body: motivation, approach, and impacts.
- PRs must include: clear description, linked issue (if any), test coverage notes, and local run steps. Add sample requests/responses when touching API endpoints.

## Security & Configuration
- Configuration: `appsettings.json` and `appsettings.Development.json`; never commit secrets.
- Use `ASPNETCORE_ENVIRONMENT` for environment-specific behavior. Keep Swagger disabled or dev-only unless justified.

## Agent-Specific Notes
- Make minimal, surgical changes; respect existing layering.
- Do not introduce new frameworks without discussion.
- Update or add tests with any behavior change; run `dotnet format` and `dotnet test` before proposing changes.
