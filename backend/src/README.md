## Running the Backend

To build the backend, navigate to the `src` folder and run:

```sh
dotnet build
```

To run all tests:

```sh
dotnet test
```

To start the main API:

```sh
cd Fundo.Applications.WebApi
dotnet run
```

The following endpoint should return **200 OK**:

```http
GET -> https://localhost:5001/loan
```

## Notes

Feel free to modify the code as needed, but try to **respect and extend the current architecture**, as this is intended to be a replica of the Fundo codebase.

---

## Implementation (added)

The solution was extended to **Clean Architecture** with four projects:

| Project | Role |
|---------|------|
| `Fundo.Domain` | Entities, business rules, domain events |
| `Fundo.Application` | Use cases, DTOs, repository interfaces |
| `Fundo.Infrastructure` | EF Core, JWT auth, migrations, seed |
| `Fundo.Applications.WebApi` | REST API, middleware |
| `Fundo.Services.Tests` | xUnit unit + integration tests (38) |

### Run locally (InMemory DB)

```bash
cd backend/src
dotnet run --project Fundo.Applications.WebApi
```

API: http://localhost:5000 — Swagger: http://localhost:5000/swagger

### Run tests

```bash
cd backend/src && dotnet test
```

### Docker

The API image is built from `backend/Dockerfile` (repo root context). See the root [README.md](../../README.md) for `docker compose up --build`.

### Implemented endpoints

| Method | Path |
|--------|------|
| `POST` | `/auth/login` |
| `GET` | `/loans` |
| `GET` | `/loans/{id}` |
| `POST` | `/loans` |
| `POST` | `/loans/{id}/payment` |
| `GET` | `/health` |

Loan endpoints require JWT (httpOnly cookie `fundo_auth`).
