# Enterprise Document Management System - API

ASP.NET Core 8.0 Web API for the Enterprise Document Management System with role-based access control, JWT authentication, and comprehensive document management features.

## Features

- **Document Management**: Upload, download, update, delete, and search documents
- **Role-Based Access Control (RBAC)**: Four-tier permission system (Viewer, Contributor, Manager, Admin)
- **Document Sharing**: Share documents with specific users with granular permissions
- **Tag Management**: Organize documents with tags
- **Access Control Levels**: Public, Private, and Restricted document access
- **Audit Logging**: Track all document operations
- **JWT Authentication**: Secure token-based authentication
- **File Storage**: Local file system storage with configurable limits
- **Validation**: FluentValidation for request validation
- **Logging**: Structured logging with Serilog
- **API Documentation**: Swagger/OpenAPI documentation

## Screenshots

### Swagger API Documentation
![Swagger API Documentation](docs/10-swagger.png)

### Database Schema
![Database Schema](docs/11-DB.png)


### Login Page
![Login Page](docs/1-login.png)

### My Documents - Dashboard
![My Documents Dashboard](docs/2.1-my-docs.png)

### Document Loading State
![Document Loading](docs/2-my-docs-loading.png)

### Search in Progress
![Search in Progress](docs/3-my-docs-search-in-progress.png)

### Search Results
![Search Results](docs/4-my-docs-search-result.png)

### Delete Document
![Delete Document](docs/5-delete.png)

### Shared With Me
![Shared With Me](docs/6-shared-with-me.png)

### Public Documents
![Public Documents](docs/7-public.png)

### Upload Document
![Upload Document](docs/8-add.png)

### Edit Document
![Edit Document](docs/9-edit.png)


## Technology Stack

- **Framework**: .NET 8.0
- **Database**: SQL Server with Entity Framework Core 8.0
- **Authentication**: JWT Bearer tokens
- **Validation**: FluentValidation
- **Logging**: Serilog
- **API Documentation**: Swashbuckle (Swagger)
- **Mapping**: AutoMapper

## Prerequisites

- .NET 8.0 SDK or later
- SQL Server (LocalDB, Express, or full version)
- Visual Studio 2022 or VS Code (optional)

## Getting Started

### 1. Database Configuration

Update the connection string in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=EnterpriseDocumentManagementDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  }
}
```

### 2. JWT Configuration

Configure JWT settings in `appsettings.json`:

```json
{
  "Jwt": {
    "Secret": "your-secret-key-at-least-32-characters-long",
    "Issuer": "EnterpriseDocumentManagementSystem",
    "Audience": "EnterpriseDocumentManagementSystem"
  }
}
```

**Important**: The JWT secret must be at least 32 characters long for security.

### 3. File Storage Configuration

Configure file storage settings in `appsettings.json`:

```json
{
  "FileStorage": {
    "Path": "DocumentStorage",
    "MaxFileSizeBytes": 10485760
  }
}
```

### 4. Database Migration

Run the following commands to create the database:

```bash
# Install EF Core tools (if not already installed)
dotnet tool install --global dotnet-ef

# Apply migrations
dotnet ef database update
```

### 5. Run the API

```bash
# Development mode
dotnet run

# Or with watch (auto-reload)
dotnet watch run
```

The API will be available at:
- HTTPS: `https://localhost:7052`
- HTTP: `http://localhost:5052`

### 6. Access Swagger Documentation

Navigate to `https://localhost:7052/swagger` to view the interactive API documentation.

## Project Structure

```
EnterpriseDocumentManagementSystem.Api/
├── Attributes/          # Custom attributes (RequireAuth)
├── Authorization/       # Authorization policies and handlers
├── Controllers/         # API controllers
│   ├── AuthController.cs
│   ├── DocumentsController.cs
│   └── TagsController.cs
├── Data/               # Database context
├── DocumentStorage/    # File storage directory
├── Extensions/         # Extension methods
├── Filters/            # Action filters (CurrentUserFilter)
├── Mappings/           # AutoMapper profiles
├── Middleware/         # Custom middleware (exception handling)
├── Migrations/         # EF Core migrations
├── Models/             # Domain models and DTOs
├── Repositories/       # Data access layer
├── Scripts/            # Database scripts
├── Services/           # Business logic services
├── Validators/         # FluentValidation validators
├── Program.cs          # Application entry point
└── appsettings.json    # Configuration
```

## API Endpoints

### Authentication (`/api/auth`)

- `POST /api/auth/login` - Login with credentials
- `GET /api/auth/me` - Get current user information
- `POST /api/auth/logout` - Logout (client-side token disposal)
- `GET /api/auth/users` - Get list of users (authenticated)

### Documents (`/api/documents`)

- `POST /api/documents/upload` - Upload a document (Contributor+)
- `GET /api/documents/{id}` - Get document by ID
- `GET /api/documents/my-documents` - Get user's documents
- `GET /api/documents/shared-with-me` - Get documents shared with user
- `GET /api/documents/public` - Get all public documents
- `POST /api/documents/search` - Search documents
- `PUT /api/documents/{id}` - Update document metadata
- `DELETE /api/documents/{id}` - Delete document
- `GET /api/documents/{id}/download` - Download document
- `POST /api/documents/share` - Share document with user
- `GET /api/documents/{documentId}/shares` - Get document shares
- `DELETE /api/documents/shares/{shareId}` - Revoke document share

### Tags (`/api/tags`)

- `GET /api/tags` - Get all tags
- `GET /api/tags/popular` - Get popular tags
- `POST /api/tags` - Create a new tag
- `GET /api/tags/{id}` - Get tag by ID
- `DELETE /api/tags/{id}` - Delete tag

## User Roles

### Viewer
- View public documents
- View documents shared with them
- Download accessible documents

### Contributor
- All Viewer permissions
- Upload new documents
- Edit own documents
- Delete own documents
- Share own documents

### Manager
- All Contributor permissions
- Manage team documents
- View team audit logs

### Admin
- All Manager permissions
- Manage all documents
- Manage users
- View all audit logs
- System configuration

## Authentication

The API uses JWT Bearer token authentication. Include the token in the Authorization header:

```
Authorization: Bearer <your-jwt-token>
```

### Mock Users for Development

The API includes a mock authentication service with the following test users:

- **Admin**: `admin@example.com` / `admin123`
- **Manager**: `manager@example.com` / `manager123`
- **Contributor**: `contributor@example.com` / `contributor123`
- **Viewer**: `viewer@example.com` / `viewer123`

## CORS Configuration

The API is configured to accept requests from:
- `http://localhost:5173` (Vite dev server)
- `http://localhost:3000` (Alternative React port)
- HTTPS variants of the above

Update the CORS policy in `Program.cs` for production deployments.

## Logging

Logs are written to:
- Console (development)
- `logs/api-{date}.log` (all logs, retained for 30 days)
- `logs/errors-{date}.log` (errors only, retained for 90 days)

## Database Schema

Key entities:
- **Document**: Core document entity with metadata
- **User**: User information (from JWT claims)
- **Tag**: Document tags
- **DocumentTag**: Many-to-many relationship
- **DocumentShare**: Document sharing permissions
- **AuditLog**: Audit trail for document operations

## Development

### Adding Migrations

```bash
# Add a new migration
dotnet ef migrations add MigrationName

# Update database
dotnet ef database update

# Rollback migration
dotnet ef database update PreviousMigrationName
```

### Running Tests

```bash
dotnet test
```

## Security Considerations

1. **JWT Secret**: Always use a strong, unique secret in production (minimum 32 characters)
2. **HTTPS**: Enable HTTPS in production
3. **CORS**: Restrict CORS origins to your frontend domain in production
4. **File Upload**: Validate file types and sizes
5. **SQL Injection**: Use parameterized queries (handled by EF Core)
6. **Authentication**: Implement proper user management system for production

## Troubleshooting

### Database Connection Issues
- Verify SQL Server is running
- Check connection string in `appsettings.json`
- Ensure database exists (run migrations)

### JWT Token Issues
- Verify JWT secret is at least 32 characters
- Check token expiration
- Ensure Issuer and Audience match configuration

### File Upload Issues
- Check `FileStorage:Path` directory exists and has write permissions
- Verify file size is within `MaxFileSizeBytes` limit

## Production Deployment

1. Update `appsettings.json` with production values
2. Use environment variables for sensitive data
3. Configure proper CORS origins
4. Enable HTTPS
5. Set up proper logging and monitoring
6. Configure database connection pooling
7. Implement rate limiting
8. Set up backup strategy for file storage and database
