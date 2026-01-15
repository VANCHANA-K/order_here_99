# Repository Update Summary

## Overview
This update adds contributor guidance, standard ignores, a quick-start README, and sets up EF Core with SQLite, including the initial migration.

## Files Added
- AGENTS.md: Repository Guidelines covering structure, commands, style, tests, PRs, and security.
- .gitignore: .NET/C# focused ignores for build outputs, IDE caches, test artifacts, and local secrets.
- README.md: Clone → build → test → run instructions for .NET 9.

## Config & Packages
- Api (`src/QrFoodOrdering.Api`)
  - Added `Microsoft.EntityFrameworkCore.Design` package.
  - Added `ConnectionStrings:Default` to `appsettings.json` and `appsettings.Development.json` (SQLite).
  - Program wires infrastructure with `AddInfrastructure(builder.Configuration.GetConnectionString("Default")!)`.
- Infrastructure (`src/QrFoodOrdering.Infrastructure`)
  - Added EF Core packages: `Microsoft.EntityFrameworkCore`, `Microsoft.EntityFrameworkCore.Sqlite`, `Microsoft.EntityFrameworkCore.Design`.
  - `AppDbContext` configured with `UseSqlite` via DI extension.

## EF Core Migration
- Generated migration: `InitialCreate` under `src/QrFoodOrdering.Infrastructure/Migrations`.
- Command used:
  ```bash
  dotnet ef migrations add InitialCreate \
    --project src/QrFoodOrdering.Infrastructure \
    --startup-project src/QrFoodOrdering.Api
  ```

## Run & Next Steps
- Build and test:
  ```bash
  dotnet build QrFoodOrdering.sln
  dotnet test QrFoodOrdering.sln
  ```
- Create database:
  ```bash
  dotnet ef database update \
    --project src/QrFoodOrdering.Infrastructure \
    --startup-project src/QrFoodOrdering.Api
  ```
- Run API:
  ```bash
  dotnet run --project src/QrFoodOrdering.Api
  ```
- SQLite files: `qrfood.db` (default), `qrfood.dev.db` (Development).
