# Repository Guidelines

## Project Structure & Module Organization
- `order_here_backend/` — ASP.NET Core (.NET 9) with Clean Architecture
  - Source: `src/` (`QrFoodOrdering.Api`, `Application`, `Domain`, `Infrastructure`)
  - Tests: `tests/`
- `order_here_frontend/` — Next.js (App Router) TypeScript app
- CI workflows: `.github/workflows/`
- Note: directory‑specific `AGENTS.md` files may exist and take precedence within their folder tree.

## Build, Test, and Development Commands
Backend:
- Restore: `cd order_here_backend && dotnet restore QrFoodOrdering.sln`
- Build: `dotnet build QrFoodOrdering.sln`
- Run API: `dotnet run --project src/QrFoodOrdering.Api/QrFoodOrdering.Api.csproj` (http://localhost:5132)
- Test: `dotnet test QrFoodOrdering.sln`
- Format: `dotnet format`

Frontend:
- Install deps: `cd order_here_frontend && npm install`
- Dev server: `npm run dev` (http://localhost:3000)
- Build/Start: `npm run build && npm start`
- Lint: `npm run lint`

## Coding Style & Naming Conventions
- C# (backend): 4‑space indent; braces on new lines; `PascalCase` for types/members; `camelCase` for locals/params; interfaces prefixed `I`; async methods end with `Async`. Run `dotnet format` before committing.
- TypeScript/React (frontend): 2‑space indent; functional components; `PascalCase` components; `camelCase` variables; colocate small UI pieces under `order_here_frontend/components/`. Use ESLint.

## Testing Guidelines
- Backend: xUnit under `order_here_backend/tests`, mirroring source namespaces. Name tests `Method_State_Expected`. Run with `dotnet test` and keep failures at 0.
- Frontend: no unit tests yet; rely on ESLint and type checks (`npx tsc --noEmit`).

## Commit & Pull Request Guidelines
- Use Conventional Commits: `feat:`, `fix:`, `chore:`, `docs:`, `ci:` (scopes allowed, e.g., `feat(api): create order endpoint`).
- PRs include: clear summary, linked issues, testing steps (e.g., `curl http://localhost:5132/health`), screenshots for UI, and notes on breaking changes/migrations. Ensure CI (restore/build/test) is green.

## Security & Configuration Tips
- Never commit secrets. Backend uses `ASPNETCORE_ENVIRONMENT` and `appsettings.*.json`; frontend uses `.env.local`.
- Dev DB is SQLite; do not commit database files. Rotate any leaked keys immediately.

## Agent‑Specific Notes (optional)
- If you use automated agents, prefer `rg` for search and read files in ≤250‑line chunks.
- Obey directory‑scoped `AGENTS.md`; more‑nested files take precedence.

