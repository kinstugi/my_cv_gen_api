# My CV Gen API

A .NET 9 Web API for CV/Resume generation with user authentication, PostgreSQL, and Redis caching.

## Tech Stack

- .NET 9
- ASP.NET Core Web API
- PostgreSQL (via Entity Framework Core)
- Redis (distributed caching)
- Docker & Docker Compose

## Prerequisites

- .NET 9 SDK
- Docker & Docker Compose (for containerized runs)
- PostgreSQL (if running locally without Docker)

## Configuration

### Local Development

- **Connection string**: Set in `appsettings.Development.json` (default: `localhost:5432`, database `my_cv_gen_api`)
- **Redis**: `localhost:6379` (optional; caching is disabled if not configured)

### Environment Variables (Docker / Render)

- `POSTGRES_PASSWORD` – PostgreSQL password (default: `postgres`)
- `Jwt__Key` – JWT signing key (min 32 characters for HS256); required for auth
- `Jwt__Issuer` – JWT issuer (default: `my_cv_gen_api`)
- `Jwt__Audience` – JWT audience (default: `my_cv_gen_api`)

## Running the API

### With Docker (Deployment)

```bash
docker compose up -d
```

API: http://localhost:8080

### With Docker (Test Environment)

Uses a separate database (`my_cv_gen_api_test`) and Redis volume:

```bash
docker compose -f docker-compose.yml -f docker-compose.test.yml up -d
```

### Locally (without Docker)

1. Start PostgreSQL and Redis (or use Docker for DB/Redis only).
2. Run:

```bash
dotnet restore
dotnet run
```

## API Endpoints

### Auth

| Method | Endpoint        | Description              |
|--------|-----------------|--------------------------|
| POST   | `/api/auth/register` | Register a new user; returns JWT + UserResponseDto |
| POST   | `/api/auth/login`    | Login; returns JWT + UserResponseDto |

Protected endpoints require `Authorization: Bearer <token>` header.

### OpenAPI

When running in Development, OpenAPI docs are available at `/openapi/v1.json`.

## Project Structure

- `Controllers/` – API controllers
- `Data/` – DbContext and EF configuration
- `DTOs/` – Data transfer objects
- `Models/` – Domain entities (User, Resume, Education, etc.)
- `Repositories/` – Data access layer
- `Services/` – Application services (e.g. password hashing)
