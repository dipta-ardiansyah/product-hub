# ProductHub — Product Management System (.NET 8)

A production-ready Product Management Web Application and REST API built with **.NET 8**, following **Clean Architecture**, **CQRS (MediatR)**, **ASP.NET Core Identity & JWT Authentication**, **Redis Caching**, and **Serilog Structured Logging**.

---

## 1. Tech Stack & Architecture

- **Runtime & Framework:** .NET 8 (`net8.0`), C# 12, ASP.NET Core MVC + Web API
- **Architecture Pattern:** Clean Architecture + CQRS with MediatR
- **Database & ORM:** PostgreSQL + Entity Framework Core 8 (Npgsql)
- **Authentication & Security:** ASP.NET Core Identity + JWT Bearer Authentication
- **Caching:** Redis via `IDistributedCache` (StackExchange.Redis) with version-based invalidation
- **Logging:** Serilog (Console & Rolling File Sink)
- **Validation:** Data Annotations with MediatR Pipeline Behavior (`ValidationBehavior`)
- **Error Handling:** RFC 7807 ProblemDetails via .NET 8 `IExceptionHandler` (`GlobalExceptionHandler`)
- **Frontend UI:** Razor Views (C# MVC View Engine) + Bootstrap 5 + Vanilla JS `fetch`
- **Testing:** xUnit, Moq, FluentAssertions, Coverlet
- **Containerization:** Docker multi-stage build & Docker Compose

---

## 2. Project Structure

```
src/
  ProductHub.Domain          # Pure domain entities (Product)
  ProductHub.Application     # CQRS Commands/Queries, Handlers, DTOs, Pipeline Behaviors, Interfaces
  ProductHub.Infrastructure  # EF Core DbContext, Repositories, Identity, JWT, Redis Caching
  ProductHub.Web             # Web API Controllers, MVC Controllers, Razor Views, wwwroot JS/CSS, Program.cs
tests/
  ProductHub.UnitTests       # xUnit unit tests for Controllers, Handlers, Behaviors, Services
```

---

## 3. Assumptions

1. **Database:** PostgreSQL is selected for high performance, cross-platform support, and seamless Docker integration.
2. **Access Control:** All product management endpoints require JWT authentication. User registration and login endpoints are public.
3. **Role & Permissions:** Default role is `User`. All authenticated users are authorized to perform product CRUD operations.
4. **Server Timestamp:** `CreatedAt` is generated server-side in UTC and cannot be modified by the client.
5. **Token Storage (UI):** JWT tokens are stored in browser `sessionStorage` for assessment simplicity (production improvement: HttpOnly Secure Cookies).
6. **Deletion:** Hard delete is implemented for products (soft delete can be introduced as an improvement).

---

## 4. Quick Start (Run in ≤ 5 Steps)

### Option A: Run with Docker Compose (Recommended)

1. Clone repository and navigate to root directory:
   ```bash
   git clone <repo-url> && cd ProductHub
   ```
2. Copy environment configuration:
   ```bash
   cp .env.example .env
   ```
3. Start all services (App, PostgreSQL, Redis):
   ```bash
   docker compose up -d --build
   ```
4. Open the Web Application UI in your browser:
   - **UI:** [http://localhost:8080](http://localhost:8080)
   - **Swagger API Docs:** [http://localhost:8080/swagger](http://localhost:8080/swagger)
   - **Health Check:** [http://localhost:8080/health](http://localhost:8080/health)

---

### Option B: Run Locally (.NET CLI + Docker dependencies)

1. Start database and cache dependencies:
   ```bash
   docker compose up -d postgres redis
   ```
2. Build solution:
   ```bash
   dotnet build -warnaserror
   ```
3. Run unit tests:
   ```bash
   dotnet test
   ```
4. Start Web Application:
   ```bash
   dotnet run --project src/ProductHub.Web
   ```
5. Navigate to `http://localhost:5000` (or the URL displayed in the console).

---

## 5. API Endpoints Contract

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| `POST` | `/api/auth/register` | Public | Register new user account |
| `POST` | `/api/auth/login` | Public | Authenticate user & receive JWT token |
| `GET` | `/api/products` | JWT | Get paged products with search & price filters |
| `GET` | `/api/products/{id}` | JWT | Get product details by ID |
| `POST` | `/api/products` | JWT | Create new product |
| `PUT` | `/api/products/{id}` | JWT | Update existing product |
| `DELETE`| `/api/products/{id}` | JWT | Delete product |
| `GET` | `/health` | Public | Health check status |

---

## 6. Running Tests

Execute all xUnit unit tests:
```bash
dotnet test
```

Execute tests with code coverage:
```bash
dotnet test --collect:"XPlat Code Coverage"
```
