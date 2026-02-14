# My CV Gen API

A .NET 9 Web API for CV/Resume generation with user authentication (JWT), PostgreSQL, and Redis caching. Users can register, log in, and manage their own resumes (create, read, update, soft delete).

## Tech Stack

- .NET 9
- ASP.NET Core Web API
- JWT (Bearer) authentication
- PostgreSQL (Entity Framework Core)
- Redis (distributed caching)
- Docker & Docker Compose

## Prerequisites

- .NET 9 SDK
- Docker & Docker Compose (for containerized runs)
- PostgreSQL (if running locally without Docker)

## Configuration

### Local Development

- **Connection string**: `appsettings.Development.json` (default: `localhost:5432`, database `my_cv_gen_api`)
- **Redis**: `localhost:6379` (optional; caching disabled if not set)
- **JWT**: Set `Jwt:Key` (min 32 characters) in `appsettings.Development.json`

### Environment Variables (Docker / Render)

- `ConnectionStrings__DefaultConnection` – PostgreSQL connection string (required on Render; use Internal Database URL)
- `POSTGRES_PASSWORD` – PostgreSQL password for Docker Compose (default: `postgres`)
- `Jwt__Key` – JWT signing key (min 32 characters); required for auth
- `Jwt__Issuer` – JWT issuer (default: `my_cv_gen_api`)
- `Jwt__Audience` – JWT audience (default: `my_cv_gen_api`)
- `Tailor__ApiKey` – Groq API key (required for CV tailor endpoint; get from [Groq Console](https://console.groq.com/))
- `Tailor__Model` – Groq model (default: `llama-3.3-70b-versatile`)

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

### Auth (no token required)

| Method | Endpoint              | Description |
|--------|------------------------|-------------|
| POST   | `/api/auth/register`   | Register; body: `firstName`, `lastName`, `email`, `password`. Returns JWT + user. |
| POST   | `/api/auth/login`      | Login; body: `email`, `password`. Returns JWT + user. Invalid credentials → 404. |

### Resumes (require `Authorization: Bearer <token>`)

| Method | Endpoint           | Description |
|--------|--------------------|-------------|
| GET    | `/api/resumes`     | List current user's active resumes (query: `page`, `pageSize`) |
| GET    | `/api/resumes/{id}`| Get resume by id (active only) |
| POST   | `/api/resumes`     | Create a resume for the current user |
| PUT    | `/api/resumes/{id}`| Update a resume (only if owned by current user) |
| DELETE | `/api/resumes/{id}`| Soft-delete a resume (only if owned by current user) |
| GET    | `/api/resumes/{id}/download` | Download resume as PDF (query: `template` = template1–4) |
| POST   | `/api/resumes/{id}/tailor` | Tailor resume to job description (body: `jobDescription`, `createNewCV` = false) |

### Other

| Method | Endpoint  | Description     |
|--------|-----------|-----------------|
| GET    | `/health` | Health check    |

### OpenAPI

In Development, OpenAPI is available at `/openapi/v1.json`.

## Project Structure

- `Controllers/` – Auth and Resume API controllers
- `Data/` – DbContext and EF configuration
- `DTOs/` – Request/response DTOs (User, Resume, Education, WorkExperience, etc.)
- `Exceptions/` – Custom exceptions (e.g. NotFoundException)
- `Models/` – Domain entities (User, Resume, Education, WorkExperience, Project, Language)
- `Repositories/` – Data access (UserRepository, ResumeRepository)
- `Services/` – JWT generation, password hashing
