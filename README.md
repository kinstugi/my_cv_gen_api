# My CV Gen API

A .NET 9 Web API for CV/Resume generation with user authentication (JWT), PostgreSQL, Redis caching, and AI-powered resume tailoring. Users can register, log in, manage resumes (create, read, update, soft delete), tailor resumes to job descriptions using Groq AI, and download resumes as PDF in multiple templates.

**Live API:** [https://my-cv-gen-api.onrender.com](https://my-cv-gen-api.onrender.com)

## Features

- **User authentication** – Register and log in with JWT Bearer tokens; optional profile fields (phone, location, GitHub, website) stored per user
- **Resume CRUD** – Create, read, update, and soft-delete resumes
- **AI resume tailoring** – Tailor a resume to match a job description using Groq AI (LLaMA)
- **PDF export** – Download resumes as PDF in 4 template styles; PDFs include the user’s name, email, phone, location, GitHub, website, and profile image when set (templates 1, 2, and 4 show the image; template 3 is text-only)
- **Resume structure** – Work experience, education, languages, projects, and skills

## Tech Stack

- .NET 9
- ASP.NET Core Web API
- JWT (Bearer) authentication
- PostgreSQL (Entity Framework Core)
- Redis (distributed caching)
- Groq AI (resume tailoring)
- QuestPDF (PDF generation)
- Docker & Docker Compose

## Prerequisites

- .NET 9 SDK
- Docker & Docker Compose (for containerized runs)
- PostgreSQL (if running locally without Docker)

---

## API Documentation

**Base URL:** `https://my-cv-gen-api.onrender.com` (production) or `http://localhost:8080` (local)

### Authentication

Most endpoints require a JWT Bearer token in the `Authorization` header:

```
Authorization: Bearer <your-jwt-token>
```

Obtain a token by registering or logging in via the auth endpoints.

---

### Auth Endpoints (no token required)

#### POST `/api/auth/register`

Register a new user and receive a JWT token.

**Request body:**

| Field       | Type   | Required | Description                              |
|-------------|--------|----------|------------------------------------------|
| firstName   | string | Yes      | User's first name                        |
| lastName    | string | Yes      | User's last name                         |
| email       | string | Yes      | Unique email address                     |
| password    | string | Yes      | User's password                          |
| phoneNumber | string | No       | User's phone number                      |
| githubUrl   | string | No       | GitHub profile URL                       |
| location    | string | No       | Location (e.g. city, country)            |
| website     | string | No       | Personal or portfolio website URL        |

**Example:**

```json
{
  "firstName": "Jane",
  "lastName": "Doe",
  "email": "jane@example.com",
  "password": "SecurePass123",
  "phoneNumber": "+1 234 567 8900",
  "githubUrl": "https://github.com/janedoe",
  "location": "London, UK",
  "website": "https://janedoe.dev"
}
```

**Response (201):**

```json
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "user": {
    "id": 1,
    "firstName": "Jane",
    "lastName": "Doe",
    "email": "jane@example.com",
    "createdAt": "2025-02-07T10:00:00Z",
    "updatedAt": "2025-02-07T10:00:00Z",
    "isActive": true,
    "phoneNumber": "+1 234 567 8900",
    "githubUrl": "https://github.com/janedoe",
    "location": "London, UK",
    "website": "https://janedoe.dev"
  }
}
```

**Errors:** `409 Conflict` – email already exists

---

#### POST `/api/auth/login`

Log in and receive a JWT token.

**Request body:**

| Field    | Type   | Required | Description |
|----------|--------|----------|-------------|
| email    | string | Yes      | User's email |
| password | string | Yes      | User's password |

**Example:**

```json
{
  "email": "jane@example.com",
  "password": "SecurePass123"
}
```

**Response (200):**

```json
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "user": {
    "id": 1,
    "firstName": "Jane",
    "lastName": "Doe",
    "email": "jane@example.com",
    "createdAt": "2025-02-07T10:00:00Z",
    "updatedAt": "2025-02-07T10:00:00Z",
    "isActive": true,
    "phoneNumber": "+1 234 567 8900",
    "githubUrl": "https://github.com/janedoe",
    "location": "London, UK",
    "website": "https://janedoe.dev"
  }
}
```

Profile fields (`phoneNumber`, `githubUrl`, `location`, `website`) are optional and may be `null` if not set.

**Errors:** `404 Not Found` – invalid email or password

---

### Resume Endpoints (require `Authorization: Bearer <token>`)

#### GET `/api/resumes`

List the current user's active resumes with pagination.

**Query parameters:**

| Parameter | Type   | Default | Description          |
|-----------|--------|---------|----------------------|
| page      | int    | 1       | Page number          |
| pageSize  | int    | 3       | Items per page       |

**Example:** `GET /api/resumes?page=1&pageSize=5`

**Response (200):** Array of `ResumeResponseDto` objects (see structure below)

---

#### GET `/api/resumes/{id}`

Get a single resume by ID (active resumes only). Accessible without ownership check (for public viewing if needed).

**Response (200):** `ResumeResponseDto` object

**Errors:** `404 Not Found` – resume not found or inactive

---

#### POST `/api/resumes`

Create a new resume for the current user.

**Request body:**

| Field            | Type                    | Required | Description                              |
|------------------|-------------------------|----------|------------------------------------------|
| title            | string                  | Yes      | Resume title (e.g. "Software Engineer")  |
| description      | string                  | Yes      | Professional summary                     |
| imageUrl         | string                  | No       | URL to profile/headshot image            |
| workExperiences  | array                   | No       | List of work experience entries          |
| educations       | array                   | No       | List of education entries                |
| languages        | array                   | No       | List of languages                        |
| projects         | array                   | No       | List of projects                         |
| skills           | array of strings        | No       | List of skills                           |

**Work experience item:**

| Field      | Type    | Required | Description                           |
|------------|---------|----------|---------------------------------------|
| company    | string  | Yes      | Company name                          |
| position   | string  | Yes      | Job title                             |
| description| string  | Yes      | Role description / achievements       |
| startDate  | string  | Yes      | ISO 8601 date (e.g. "2020-01-01")     |
| endDate    | string  | No       | ISO 8601 date or null                 |
| isCurrent  | boolean | No       | True if currently employed            |

**Education item:**

| Field       | Type   | Required | Description                    |
|-------------|--------|----------|--------------------------------|
| school      | string | Yes      | School/university name         |
| degree      | string | Yes      | Degree type (e.g. "BSc")       |
| fieldOfStudy| string | Yes      | Field of study                 |
| startDate   | string | Yes      | ISO 8601 date                  |
| endDate     | string | No       | ISO 8601 date or null          |

**Language item:**

| Field  | Type   | Required | Description (e.g. "Fluent", "Intermediate") |
|--------|--------|----------|---------------------------------------------|
| name   | string | Yes      | Language name                               |
| level  | string | Yes      | Proficiency level                           |

**Project item:**

| Field       | Type   | Required | Description                  |
|-------------|--------|----------|------------------------------|
| title       | string | Yes      | Project title                |
| description | string | Yes      | Project description          |
| link        | string | No       | URL to project (GitHub, etc.)|

**Example:**

```json
{
  "title": "Software Engineer",
  "description": "Experienced developer with 5 years in web technologies.",
  "imageUrl": "https://example.com/photo.jpg",
  "workExperiences": [
    {
      "company": "Acme Corp",
      "position": "Senior Developer",
      "description": "Led team of 5 developers.",
      "startDate": "2020-01-01",
      "endDate": "2024-12-31",
      "isCurrent": false
    }
  ],
  "educations": [
    {
      "school": "Tech University",
      "degree": "BSc",
      "fieldOfStudy": "Computer Science",
      "startDate": "2015-09-01",
      "endDate": "2019-06-30"
    }
  ],
  "languages": [
    { "name": "English", "level": "Fluent" },
    { "name": "Spanish", "level": "Intermediate" }
  ],
  "projects": [
    {
      "title": "Open Source Library",
      "description": "Contributed to popular .NET library.",
      "link": "https://github.com/example/repo"
    }
  ],
  "skills": ["C#", ".NET", "PostgreSQL", "Redis"]
}
```

**Response (200):** `ResumeResponseDto` object (includes `id`, `createdAt`, `updatedAt`, etc.)

---

#### PUT `/api/resumes/{id}`

Update an existing resume. Only the owner can update. All fields are optional; provided fields replace existing data.

**Request body:** Same structure as `ResumeCreateDto`, but all fields optional. Omitted collections (e.g. `workExperiences`) are left unchanged.

| Field            | Type                    | Required | Description                    |
|------------------|-------------------------|----------|--------------------------------|
| title            | string                  | No       | Resume title                   |
| description      | string                  | No       | Professional summary           |
| imageUrl         | string                  | No       | Profile image URL              |
| isActive         | boolean                 | No       | Resume active status           |
| workExperiences  | array                   | No       | Replaces all work experiences  |
| educations       | array                   | No       | Replaces all educations        |
| languages        | array                   | No       | Replaces all languages         |
| projects         | array                   | No       | Replaces all projects          |
| skills           | array of strings        | No       | Replaces all skills            |

**Response (200):** Updated `ResumeResponseDto` object

**Errors:** `404 Not Found` – resume not found or not owned by user

---

#### DELETE `/api/resumes/{id}`

Soft-delete a resume (sets `isActive` to false). Only the owner can delete.

**Response (200):** Deleted `ResumeResponseDto` object (still returned, but marked inactive)

**Errors:** `404 Not Found` – resume not found or not owned by user

---

#### GET `/api/resumes/{id}/download`

Download a resume as a PDF file. Only the owner can download. The PDF is built from the resume data plus the **current user’s profile**: name, email, phone, location, GitHub URL, website, and (on templates 1, 2, 4) profile image. Any of these are included only when set in the user profile or resume (e.g. `imageUrl` is on the resume). Template 3 does not display a profile image.

**Query parameters:**

| Parameter | Type   | Default   | Description                               |
|-----------|--------|-----------|-------------------------------------------|
| template  | string | template1 | PDF template: `template1`, `template2`, `template3`, `template4` |

**Example:** `GET /api/resumes/1/download?template=template2`

**Response (200):** PDF file (`application/pdf`) with filename `resume-{id}.pdf`

**Errors:** `404 Not Found` – resume not found or not owned by user

---

#### POST `/api/resumes/{id}/tailor`

Tailor a resume to match a job description using Groq AI. Rephrases descriptions and emphasizes relevant skills and achievements. Factual data (companies, dates, schools) is preserved. Only the owner can tailor.

**Request body:**

| Field          | Type   | Required | Description                                                                 |
|----------------|--------|----------|-----------------------------------------------------------------------------|
| jobDescription | string | Yes      | Full job posting or description to tailor the resume for                    |
| createNewCV    | boolean| No       | If `true`, creates a new resume; if `false` (default), updates the existing |

**Example:**

```json
{
  "jobDescription": "We are looking for a Senior Software Engineer with 5+ years experience in C#, .NET, and cloud technologies. Strong communication and team leadership skills required.",
  "createNewCV": false
}
```

**Response (200):** `ResumeResponseDto` of the tailored resume (new or updated)

**Errors:**
- `400 Bad Request` – `jobDescription` is empty
- `404 Not Found` – resume not found or not owned by user

---

### Other Endpoints

#### GET `/health`

Health check. Returns `{"status":"healthy"}`.

**Response (200):**

```json
{
  "status": "healthy"
}
```

---

### Resume Response Structure (`ResumeResponseDto`)

All resume endpoints return (or include) this structure:

```json
{
  "id": 1,
  "title": "Software Engineer",
  "description": "Experienced developer...",
  "imageUrl": "https://example.com/photo.jpg",
  "isActive": true,
  "workExperiences": [
    {
      "id": 1,
      "company": "Acme Corp",
      "position": "Senior Developer",
      "description": "Led team of 5 developers.",
      "startDate": "2020-01-01T00:00:00Z",
      "endDate": "2024-12-31T00:00:00Z",
      "isCurrent": false
    }
  ],
  "educations": [
    {
      "id": 1,
      "school": "Tech University",
      "degree": "BSc",
      "fieldOfStudy": "Computer Science",
      "startDate": "2015-09-01T00:00:00Z",
      "endDate": "2019-06-30T00:00:00Z"
    }
  ],
  "languages": [
    { "id": 1, "name": "English", "level": "Fluent" }
  ],
  "projects": [
    {
      "id": 1,
      "title": "Open Source Library",
      "description": "Contributed to popular .NET library.",
      "link": "https://github.com/example/repo"
    }
  ],
  "skills": ["C#", ".NET", "PostgreSQL"],
  "createdAt": "2025-02-07T10:00:00Z",
  "updatedAt": "2025-02-07T10:00:00Z"
}
```

**Note:** Dates use ISO 8601 format. Request bodies accept `yyyy-MM-dd` strings; responses include full UTC timestamps.

---

### User object in auth responses

Register and login return a `user` object with the following. All profile fields are optional and may be `null`.

| Field       | Type   | Description                    |
|-------------|--------|--------------------------------|
| id          | int    | User ID                        |
| firstName   | string | First name                     |
| lastName    | string | Last name                      |
| email       | string | Email address                  |
| createdAt   | string | ISO 8601 timestamp             |
| updatedAt   | string | ISO 8601 timestamp             |
| isActive    | bool   | Account active                 |
| phoneNumber | string \| null | Phone number            |
| githubUrl   | string \| null | GitHub profile URL      |
| location    | string \| null | Location (e.g. city, country) |
| website     | string \| null | Website URL                |

When a user downloads a resume PDF, these profile values (and the resume’s `imageUrl` when used) are shown on the CV where the template has contact or header sections.

---

## Configuration

### Local Development

- **Connection string**: `appsettings.Development.json` (default: `localhost:5432`, database `my_cv_gen_api`)
- **Redis**: `localhost:6379` (optional; caching disabled if not set)
- **JWT**: Set `Jwt:Key` (min 32 characters) in `appsettings.Development.json`

### Environment Variables (Docker / Render)

| Variable                            | Description                                                                 |
|-------------------------------------|-----------------------------------------------------------------------------|
| `ConnectionStrings__DefaultConnection` | PostgreSQL connection string (required on Render; use Internal Database URL) |
| `POSTGRES_PASSWORD`                 | PostgreSQL password for Docker Compose (default: `postgres`)                |
| `Jwt__Key`                          | JWT signing key (min 32 characters); required for auth                     |
| `Jwt__Issuer`                       | JWT issuer (default: `my_cv_gen_api`)                                      |
| `Jwt__Audience`                     | JWT audience (default: `my_cv_gen_api`)                                    |
| `Tailor__ApiKey`                    | Groq API key (required for tailor endpoint; get from [Groq Console](https://console.groq.com/)) |
| `Tailor__Model`                     | Groq model (default: `llama-3.3-70b-versatile`)                            |

---

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

---

## Project Structure

- `Controllers/` – Auth and Resume API controllers
- `Data/` – DbContext and EF configuration
- `DTOs/` – Request/response DTOs (User, Resume, Education, WorkExperience, etc.)
- `Exceptions/` – Custom exceptions (e.g. NotFoundException)
- `Models/` – Domain entities (User, Resume, Education, WorkExperience, Project, Language)
- `Repositories/` – Data access (UserRepository, ResumeRepository)
- `Services/` – JWT, password hashing, CV PDF generation, AI resume tailoring
- `Templates/` – PDF templates (template1–4)
