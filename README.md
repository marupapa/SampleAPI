# SampleAPI

SampleAPI is a .NET 10 Web API sample that uses a layered architecture, JWT Bearer authentication, Dapper-based SQL Server access, stored procedures, NLog, Swagger, and AWS Secrets Manager integration.

The solution uses the XML-based `.slnx` format.

## Requirements

- .NET 10 SDK
- Visual Studio 2022 17.10 or later, Visual Studio 2026, Rider, or VS Code
- SQL Server 2019 or later for local database testing
- AWS credentials only for non-Local environments that read database secrets from AWS Secrets Manager

## Solution Structure

```text
SampleAPI/
├── SampleAPI.slnx
├── Database/
│   └── InitializeDatabase.sql
├── SampleAPI/
│   ├── Areas/V1/Controllers/UserController.cs
│   ├── Handlers/GlobalExceptionHandler.cs
│   ├── Interfaces/IUserService.cs
│   ├── Models/
│   ├── Services/UserService.cs
│   ├── Program.cs
│   ├── appsettings.json
│   ├── appsettings.Local.json
│   ├── appsettings.Development.json
│   ├── appsettings.Pre.json
│   ├── appsettings.Live.json
│   └── nlog.config
├── SampleAPI.ApplicationCore/
│   ├── Configurations/
│   ├── Interfaces/IUserRepository.cs
│   └── Models/User.cs
├── SampleAPI.Common/
│   ├── Extensions/
│   ├── Helpers/
│   └── Logging/
└── SampleAPI.Infrastructure/
    ├── Configurations/SecretsManagerHelper.cs
    ├── Data/
    │   ├── DapperHelper.cs
    │   ├── ProcedureHelper.cs
    │   └── UserRepository.cs
    └── ExternalApi/
        ├── ExternalApiClient.cs
        └── IExternalApiClient.cs
```

## Architecture

The solution is split into four projects.

- `SampleAPI`: presentation layer. It contains controllers, DTOs, service implementations, middleware registration, Swagger, JWT setup, health endpoints, and application startup.
- `SampleAPI.ApplicationCore`: application contracts and domain models.
- `SampleAPI.Infrastructure`: SQL Server access, stored procedure execution, external HTTP API client, and AWS Secrets Manager integration.
- `SampleAPI.Common`: shared logging, helpers, and extension methods.

## Configuration

`appsettings.json` contains shared configuration. Its `JwtSettings:SecretKey` value is intentionally empty and must be provided by environment-specific configuration or environment variables.

`appsettings.Local.json` contains a local development JWT secret and a local SQL Server connection string example.

For `Development`, `Pre`, and `Live`, database connection strings are loaded through AWS Secrets Manager first. If Secrets Manager cannot be read, the application falls back to configured connection strings.

Recommended environment variable names follow ASP.NET Core configuration conventions:

```bash
JwtSettings__SecretKey="replace-with-secure-secret"
ConnectionStrings__DefaultConnection="Server=...;Database=...;User Id=...;Password=...;TrustServerCertificate=True;"
```

## Local Database Setup

Create the local database first:

```sql
CREATE DATABASE SampleDB_Local;
GO

USE SampleDB_Local;
GO
```

Then run:

```text
Database/InitializeDatabase.sql
```

The script creates:

- `Users` table
- `sp_CreateUser`
- `sp_UpdateUser`
- `sp_DeleteUser`
- sample users

## Build

From the repository root:

```bash
dotnet restore SampleAPI.slnx
dotnet build SampleAPI.slnx
```

## Run Locally

Using the launch profile:

```bash
dotnet run --project SampleAPI/SampleAPI.csproj --launch-profile http
```

Or explicitly:

```bash
ASPNETCORE_ENVIRONMENT=Local dotnet run --project SampleAPI/SampleAPI.csproj --urls http://127.0.0.1:5000
```

Swagger is enabled in `Local` and `Development` environments:

```text
http://localhost:5000/swagger
```

## Health Checks

Health endpoints are anonymous and do not require JWT authentication:

```bash
curl http://localhost:5000/health
curl http://localhost:5000/api/v1/health
```

Example response:

```json
{
  "status": "Healthy",
  "timestamp": "2026-05-03T00:00:00Z"
}
```

## API Endpoints

All user endpoints require a valid JWT Bearer token:

```text
Authorization: Bearer {jwt-token}
```

Available endpoints:

- `GET /api/v1/user`
- `GET /api/v1/user/{id}`
- `POST /api/v1/user`
- `PUT /api/v1/user/{id}`
- `DELETE /api/v1/user/{id}`

User creation request:

```json
{
  "username": "testuser",
  "email": "test@example.com",
  "fullName": "Test User",
  "phoneNumber": "090-1234-5678",
  "password": "Password123!"
}
```

The password is hashed with ASP.NET Core `IPasswordHasher<User>` before being passed to the repository.

## Authentication

The application uses ASP.NET Core JWT Bearer authentication configured in `Program.cs`.

There is no custom sample authentication handler. A caller must provide a real JWT signed with `JwtSettings:SecretKey` and matching:

- `JwtSettings:Issuer`
- `JwtSettings:Audience`

Swagger is configured with a Bearer security definition, so authenticated calls can be tested through the Swagger UI after a valid JWT is available.

## Data Access

Reads use Dapper SQL queries through `DapperHelper`.

Writes use stored procedures through `ProcedureHelper`:

- create: `sp_CreateUser`
- update: `sp_UpdateUser`
- delete: `sp_DeleteUser`

Database connection strings are resolved asynchronously at query/procedure execution time instead of blocking during DI construction.

## External API Client

`ExternalApiClient` is registered as a typed HTTP client:

```csharp
builder.Services.AddHttpClient<IExternalApiClient, ExternalApiClient>();
```

This avoids manually constructing `HttpClient` and lets ASP.NET Core manage the underlying handlers.

## CI/CD

`Jenkinsfile` uses the `.slnx` solution file:

```bash
dotnet restore SampleAPI.slnx
dotnet build SampleAPI.slnx --configuration Release --no-restore
dotnet test SampleAPI.slnx --configuration Release --no-build
```

The pipeline smoke tests call:

- `/health`
- `/api/v1/health`

## Environments

| Environment | Purpose | Swagger | Database secret source |
| --- | --- | --- | --- |
| Local | Local development | Enabled | `appsettings.Local.json` |
| Development | Shared development | Enabled | AWS Secrets Manager, then fallback config |
| Pre | Staging | Disabled | AWS Secrets Manager, then fallback config |
| Live | Production | Disabled | AWS Secrets Manager, then fallback config |

## Project Highlights

- .NET 10
- `.slnx` solution format
- Layered architecture
- JWT Bearer authentication
- Swagger/OpenAPI
- Global exception handling
- NLog logging
- Dapper reads
- Stored procedure writes
- AWS Secrets Manager support
- Typed `HttpClientFactory` external API client
- Anonymous health endpoints for deployment checks

## Current Notes

- No test project is currently included.
- The API validates JWTs but does not currently provide a login/token issuing endpoint.
- Configure production JWT secrets outside source-controlled files.
