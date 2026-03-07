# IdeaBoard

A .NET 8 backend implementing a clean, layered architecture for an ideas/voting/review system with authentication, notifications, reporting, and role-based user management.

## Table of contents
- [Project Overview](#project-overview)
- [Architecture & Dataflow](#architecture--dataflow)
- [Project Structure](#project-structure)
- [Key Components & Responsibilities](#key-components--responsibilities)
- [Authentication & Authorization](#authentication--authorization)
- [Configuration & Environment](#configuration--environment)
- [Development — Setup & Commands](#development--setup--commands)

## Project Overview
`IdeaBoard` is a .NET 8 Web API implementing the backend for an ideas platform. It supports:
- User authentication and role management
- Idea creation, commenting and voting
- Review workflows with decisions
- Notification delivery and status updates
- Reporting features

The codebase follows separation of concerns: Controllers, Services, Repositories, Domain models, DTOs and Middleware.

## Architecture & Dataflow
High-level request flow:
1. Client issues an HTTP request to an endpoint (e.g. `POST /api/auth/login`).
2. Routing dispatches to a Controller (e.g. `AuthController`).
3. The Controller validates input, maps DTOs and calls a Service (e.g. `TokenService`, `ReportsService`).
4. Services contain business logic and orchestrate work, calling Repositories for data access (e.g. `ReportsRepository`, `AuthRepository`).
5. Repositories interact with the database using EF Core migrations (migration files under `Migrations`).
6. Results propagate back: Repository → Service → Controller → HTTP response.
7. Cross-cutting concerns (exception handling, logging) are handled with middleware and DI configuration in `Program.cs`.

This layered flow keeps controllers thin and business logic testable in Services.

## Project Structure
Top-level folders you’ll find:
- `Controllers/` — route handlers (e.g. `AuthController`, `IdeaController`, `CommentController`, `NotificationController`, `ReportsController`, `UserManagementController`).
- `Services/` — business logic classes and interfaces (e.g. `ReportsService`, `ReviewService`, `NotificationService`, `TokenService`).
- `Repositories/` — data access implementations and interfaces (e.g. `IReportsRepository`, `ReportsRepository`, `AuthRepository`, `CategorieRepository`).
- `Models/Domain/` — EF Core domain entities (e.g. `User`, `Idea`, `Vote`, `Notification`).
- `Models/DTO/` — request/response DTOs organized by feature (Auth, User, Idea, Vote, Review, Notification, etc.).
- `Middlewares/` — global middleware such as `ExceptionHandlerMiddleware`.
- `Migrations/` — EF Core migration files (e.g. `20260213040705_AddNotificationForeignKeyColumns.cs`).
- `Program.cs` — app startup, DI, middleware registration and configuration.
- `appsettings.json` — configuration (connection strings, JWT settings, logging).

## Key Components & Responsibilities
- Controllers
  - Handle routing, authentication attributes, request/response mapping.
  - Should remain thin; call Services for business rules.
- Services (e.g. `IReportsService`, `ITokenService`, `INotificationService`)
  - Implement core business workflows and validations.
  - Coordinate repositories and other services.
- Repositories (e.g. `IAuthRepository`, `IReportsRepository`)
  - Encapsulate EF Core queries and persistence logic.
  - Return domain models or DTOs suitable for the Service layer.
- Models
  - `Domain` objects map to database tables.
  - `DTO` objects define API contracts (trim the domain for network transfer).
- Middlewares
  - `ExceptionHandlerMiddleware` standardizes error responses and logs exceptions.
- Authentication
  - `TokenService` issues JWT tokens.
  - `AuthRepository` handles user persistence and credential checks.
- Migrations
  - EF Core migrations maintain schema (apply via `dotnet ef` or through programmatic migrations).

## Authentication & Authorization
- JWT-based authentication is implemented via `TokenService`.
- Role-based access is supported (see DTOs like `UpdateUserRoleRequestDto` and `UserManagementController`).
- Protect endpoints using authorization attributes in controllers.
- Keep secret keys and token lifetimes in `appsettings.json` or secure secret stores (environment variables / Azure Key Vault).

## Configuration & Environment
- Primary config file: `appsettings.json`. Do NOT check secrets into source control.
- Typical keys to set:
  - `ConnectionStrings:DefaultConnection` — EF Core database connection string
  - `Jwt:Key`, `Jwt:Issuer`, `Jwt:Audience`, `Jwt:ExpiresMinutes` — token settings
- Use environment variables for CI/CD or containerized deployments.

## Development — Setup & Commands
Prerequisites:
- .NET 8 SDK
- SQL Server
- EF Core CLI if running migrations locally: `dotnet tool install --global dotnet-ef`

Common commands:
- Restore & build
  - `dotnet restore`
  - `dotnet build`
- Run
  - `dotnet run` (from solution or web project folder)
- EF Core migrations
  - Add migration: `Add-Migration <migration_name>`
  - Apply migrations: `Update-Database`
- Run in development environment:
  - Ensure the `ASPNETCORE_ENVIRONMENT` is set to `Development` or desired profile.


Configuration tips:
- Place production secrets in environment variables or a secret manager.
- Keep `appsettings.Development.json` out of CI/CD unless safe.



