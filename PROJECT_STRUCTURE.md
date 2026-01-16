# SampleAPI Solution - Project Structure

```
SampleAPI-Solution/
│
├── SampleAPI.slnx                          # Visual Studio 2026 Solution File
├── README.md                               # Project Documentation
├── .gitignore                              # Git Ignore Configuration
├── Jenkinsfile                             # Jenkins Pipeline Configuration
│
├── Database/
│   └── InitializeDatabase.sql              # Database Schema and Stored Procedures
│
├── SampleAPI/                              # API Project (Presentation Layer)
│   ├── SampleAPI.csproj
│   ├── Program.cs                          # Application Entry Point
│   ├── nlog.config                         # NLog Configuration
│   │
│   ├── appsettings.json                    # Base Configuration
│   ├── appsettings.Local.json              # Local Environment Configuration
│   ├── appsettings.Development.json        # Development Environment Configuration
│   ├── appsettings.Pre.json                # Pre-production Environment Configuration
│   ├── appsettings.Live.json               # Production Environment Configuration
│   │
│   ├── Areas/
│   │   └── V1/
│   │       └── Controllers/
│   │           └── UserController.cs       # User API Controller
│   │
│   ├── Models/                             # DTOs and Request/Response Models
│   │   ├── UserRequestModel.cs
│   │   ├── UserResponseModel.cs
│   │   └── ApiResponseModel.cs
│   │
│   ├── Services/                           # Business Logic Implementation
│   │   └── UserService.cs
│   │
│   ├── Interfaces/                         # Service Interfaces
│   │   └── IUserService.cs
│   │
│   └── Handlers/                           # Middleware Handlers
│       ├── AuthenticationHandler.cs        # JWT Authentication Handler
│       └── GlobalExceptionHandler.cs       # Global Exception Handler
│
├── SampleAPI.ApplicationCore/              # Application Core Layer
│   ├── SampleAPI.ApplicationCore.csproj
│   │
│   ├── Models/                             # Domain Entities
│   │   └── User.cs
│   │
│   ├── Interfaces/                         # Repository Interfaces
│   │   └── IUserRepository.cs
│   │
│   └── Configurations/                     # Configuration Models
│       ├── DatabaseConfiguration.cs
│       └── ApiConfiguration.cs
│
├── SampleAPI.Common/                       # Common Library Layer
│   ├── SampleAPI.Common.csproj
│   │
│   ├── Logging/                            # Logging Components
│   │   ├── ILoggerService.cs
│   │   └── NLogService.cs                  # NLog Implementation
│   │
│   ├── Extensions/                         # Extension Methods
│   │   ├── StringExtensions.cs
│   │   └── DateTimeExtensions.cs
│   │
│   └── Helpers/                            # Helper Classes
│       └── JsonHelper.cs
│
└── SampleAPI.Infrastructure/               # Infrastructure Layer
    ├── SampleAPI.Infrastructure.csproj
    │
    ├── Data/                               # Data Access Layer
    │   ├── IDapperHelper.cs                # Dapper Helper Interface
    │   ├── DapperHelper.cs                 # Dapper Implementation for Reads
    │   ├── IProcedureHelper.cs             # Procedure Helper Interface
    │   ├── ProcedureHelper.cs              # Stored Procedure Implementation for CUD
    │   └── UserRepository.cs               # User Repository Implementation
    │
    ├── ExternalApi/                        # External API Integration
    │   ├── IExternalApiClient.cs
    │   └── ExternalApiClient.cs            # HTTP Client for External APIs
    │
    └── Configurations/                     # Infrastructure Configuration
        └── SecretsManagerHelper.cs         # AWS Secrets Manager Integration

```

## File Count Summary

- **Total C# Files**: 29
- **Configuration Files**: 6
- **Documentation Files**: 2
- **Build/Deploy Files**: 2

## Layer Responsibilities

### 1. SampleAPI (Presentation Layer)
- HTTP request/response handling
- Authentication & Authorization
- API routing and versioning
- Input validation
- Swagger documentation

### 2. SampleAPI.ApplicationCore (Domain Layer)
- Business logic interfaces
- Domain models/entities
- Application contracts
- Configuration definitions

### 3. SampleAPI.Infrastructure (Infrastructure Layer)
- Database access (Dapper for reads, Stored Procedures for writes)
- External API integration
- AWS Secrets Manager integration
- Repository implementations

### 4. SampleAPI.Common (Shared Library)
- Cross-cutting concerns
- Logging infrastructure (NLog)
- Extension methods
- Helper utilities

## Key Features Implemented

✅ 4-Layer Clean Architecture
✅ JWT Authentication with Authorization Header
✅ Swagger/OpenAPI Documentation
✅ Global Exception Handling
✅ NLog Structured Logging
✅ Dapper for Database Reads
✅ Stored Procedures for Database Writes
✅ AWS Secrets Manager Integration
✅ Multi-Environment Support (Local/Development/Pre/Live)
✅ Jenkins CI/CD Pipeline
✅ RESTful API Design
✅ Dependency Injection
✅ Repository Pattern
✅ External API Client

## Getting Started

1. Open `SampleAPI.slnx` in Visual Studio 2026
2. Restore NuGet packages
3. Configure connection strings in `appsettings.json` or AWS Secrets Manager
4. Run database initialization script: `Database/InitializeDatabase.sql`
5. Set `ASPNETCORE_ENVIRONMENT` environment variable
6. Run the application

## Build & Deploy

```bash
# Restore dependencies
dotnet restore

# Build solution
dotnet build SampleAPI.sln --configuration Release

# Run tests
dotnet test

# Publish
dotnet publish SampleAPI/SampleAPI.csproj --configuration Release

# Deploy via Jenkins
# Use Jenkinsfile with environment parameter: Local/Development/Pre/Live
```

## API Endpoints

- `GET /api/v1/user` - Get all users
- `GET /api/v1/user/{id}` - Get user by ID
- `POST /api/v1/user` - Create new user
- `PUT /api/v1/user/{id}` - Update user
- `DELETE /api/v1/user/{id}` - Delete user

All endpoints require `Authorization: Bearer {token}` header.

## Environment Configuration

Each environment has its own configuration file:
- **Local**: `appsettings.Local.json`
- **Development**: `appsettings.Development.json`
- **Pre**: `appsettings.Pre.json`
- **Live**: `appsettings.Live.json`

Set the `ASPNETCORE_ENVIRONMENT` variable to switch between environments.
