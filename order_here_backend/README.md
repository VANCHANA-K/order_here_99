# Order Here – Backend (QR Food Ordering Platform)

A production-grade QR Code Food Ordering Backend built with:

- ASP.NET Core (.NET 9)
- Clean Architecture (Domain / Application / Infrastructure / API)
- SQLite (Development)
- xUnit (Testing)
- GitHub Actions CI

---

# 🏗 Repository Structure

```
order_here/
├── order_here_backend/
│   ├── QrFoodOrdering.sln
│   ├── src/
│   │   ├── QrFoodOrdering.Api
│   │   ├── QrFoodOrdering.Application
│   │   ├── QrFoodOrdering.Domain
│   │   └── QrFoodOrdering.Infrastructure
│   └── tests/
│       └── QrFoodOrdering.Tests
│
└── .github/workflows/
    └── ci.yml
```

---

# 🔧 Prerequisites

- .NET SDK 9.x
- Git

Check installed SDK:

```bash
dotnet --version
```

---

# 🚀 Clone Repository

```bash
git clone https://github.com/VANCHANA-K/order_here_99.git
cd order_here/order_here_backend
```

---

# 🟢 Run Backend

# 🧪 Run Tests

From `order_here_backend`:



Restore:

```bash
dotnet restore QrFoodOrdering.sln
```

```bash

dotnet test QrFoodOrdering.sln
```

Build:

```bash
dotnet build QrFoodOrdering.sln
```

Run API:

```bash
dotnet run --project src/QrFoodOrdering.Api/QrFoodOrdering.Api.csproj
```

API runs at:

```
http://localhost:5132
```

---

# 🧪 Run Tests

From `order_here_backend`:

```bash
dotnet test QrFoodOrdering.sln
```

Expected result:

```
Test summary: total: X, failed: 0
```

All tests must pass before merging.

---

# 🟢 Health Check

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

---

# 🧠 Architecture

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

---

# 🔁 CI Pipeline

GitHub Actions runs:

- dotnet restore
- dotnet build
- dotnet test

Target solution:

```
order_here_backend/QrFoodOrdering.sln
```

CI must pass before merge to main.

---

# 📍 Current Scope (Sprint 3 – Day 1)

Table Management (Staff Foundation)

- Create Table API
- Update Table Status
- Table validation
- Audit logging

System verified with:

- Successful build
- Passing test suite
- Health endpoint validation

---

# 🔐 Environment

- Development (default)
- Test (CI)

No secrets stored in repository.
SQLite is used for development only.

---

# 👨‍💻 Maintainer

Vanchana Karoon  
Production-Grade Clean Architecture System
