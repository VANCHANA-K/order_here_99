# Order Here – QR Food Ordering Platform

A production-grade QR Code Food Ordering Platform built with:

- ASP.NET Core (.NET 9)
- Clean Architecture (Domain / Application / Infrastructure / API)
- SQLite (Development)
- xUnit (Testing)
- GitHub Actions CI
- Next.js (Frontend – Sprint 3)

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
├── order_here_frontend/   (Next.js App Router)
│
└── .github/workflows/
    └── ci.yml
```

---

# 🔧 Prerequisites

## Backend
- .NET SDK 9.x
- Git

## Frontend
- Node.js 18+
- npm

---

# 🚀 Clone Repository

```bash
git clone https://github.com/VANCHANA-K/order_here_99.git
cd order_here_99
```

---

# 🟢 Run Backend

Navigate to backend:

```bash
cd order_here_backend
```

Restore:

```bash
dotnet restore QrFoodOrdering.sln
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

---

# 🌐 Run Frontend (Sprint 3)

Navigate:

```bash
cd order_here_frontend
```

Install:

```bash
npm install
```

Run:

```bash
npm run dev
```

Frontend runs at:

```
http://localhost:3000
```

Staff Table Page:

```
http://localhost:3000/staff/tables
```

---

# 🧠 Architecture

Backend follows Clean Architecture:

- Domain → Business rules
- Application → Use cases
- Infrastructure → EF Core + SQLite
- API → HTTP endpoints
- Tests → Domain & Application validation

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

---

# 📍 Current Sprint Status

## Sprint 3 – Day 1 (Table Management)

Backend:
- Create Table API
- Update Table Status
- Table validation
- Audit logging

Frontend:
- Table List Page (Staff)
- Create Table UI (basic)

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
