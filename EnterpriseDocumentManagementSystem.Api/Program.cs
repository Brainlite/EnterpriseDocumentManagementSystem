using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;
using EnterpriseDocumentManagementSystem.Api.Data;
using EnterpriseDocumentManagementSystem.Api.Filters;
using EnterpriseDocumentManagementSystem.Api.Middleware;
using EnterpriseDocumentManagementSystem.Api.Repositories;
using EnterpriseDocumentManagementSystem.Api.Repositories.Interfaces;
using EnterpriseDocumentManagementSystem.Api.Services;
using EnterpriseDocumentManagementSystem.Api.Models;
using EnterpriseDocumentManagementSystem.Api.Authorization;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()  // Show debug logs in development
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Information)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "EnterpriseDocumentManagement")
    .Enrich.WithProperty("Environment", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production")
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/api-.log",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}",
        retainedFileCountLimit: 30)
    .WriteTo.File(
        path: "logs/errors-.log",
        restrictedToMinimumLevel: LogEventLevel.Error,
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}",
        retainedFileCountLimit: 90)
    .CreateLogger();

try
{
    Log.Information("Starting Enterprise Document Management System API");

var builder = WebApplication.CreateBuilder(args);

// Use Serilog for logging
builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddControllers();

// Register CurrentUserFilter for dependency injection
builder.Services.AddScoped<CurrentUserFilter>();

// Configure Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null);
    }));

// Register Repositories
builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();
builder.Services.AddScoped<IDocumentShareRepository, DocumentShareRepository>();
builder.Services.AddScoped<ITagRepository, TagRepository>();
builder.Services.AddScoped<IDocumentTagRepository, DocumentTagRepository>();
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Register Services
builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<IAuthorizationService, DocumentAuthorizationService>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(
                "http://localhost:5173",  // Vite dev server
                "https://localhost:5173", // Vite dev server HTTPS
                "http://localhost:3000",  // Alternative React port
                "https://localhost:3000", // Alternative React port HTTPS
                "http://127.0.0.1:5173",
                "https://127.0.0.1:5173",
                "http://127.0.0.1:3000",
                "https://127.0.0.1:3000"
              )
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
    
    // Named policy for production use
    options.AddPolicy("ProductionPolicy", policy =>
    {
        //TODO: change this
        policy.WithOrigins(
                "http://localhost",
                "https://localhost"
              )
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Add FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// Register MockAuthService
builder.Services.AddSingleton<MockAuthService>();

// Configure JWT Authentication - Throw error if Secret is not configured
var jwtSecret = builder.Configuration["Jwt:Secret"] 
    ?? throw new InvalidOperationException("JWT Secret is not configured. Please set 'Jwt:Secret' in appsettings.json");

if (jwtSecret.Length < 32)
{
    throw new InvalidOperationException("JWT Secret must be at least 32 characters long for security reasons.");
}

var jwtIssuer = builder.Configuration["Jwt:Issuer"] 
    ?? throw new InvalidOperationException("JWT Issuer is not configured. Please set 'Jwt:Issuer' in appsettings.json");

var jwtAudience = builder.Configuration["Jwt:Audience"] 
    ?? throw new InvalidOperationException("JWT Audience is not configured. Please set 'Jwt:Audience' in appsettings.json");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// Configure Authorization with role-based policies
builder.Services.AddAuthorization(options =>
{
    // Role-based policies
    options.AddPolicy(Policies.ViewerPolicy, policy => policy.RequireRole(Roles.Viewer, Roles.Contributor, Roles.Manager, Roles.Admin));
    options.AddPolicy(Policies.ContributorPolicy, policy => policy.RequireRole(Roles.Contributor, Roles.Manager, Roles.Admin));
    options.AddPolicy(Policies.ManagerPolicy, policy => policy.RequireRole(Roles.Manager, Roles.Admin));
    options.AddPolicy(Policies.AdminPolicy, policy => policy.RequireRole(Roles.Admin));

    // Feature-based policies
    options.AddPolicy(Policies.CanCreateDocuments, policy => policy.RequireRole(Roles.Contributor, Roles.Manager, Roles.Admin));
    options.AddPolicy(Policies.CanEditOwnDocuments, policy => policy.RequireRole(Roles.Contributor, Roles.Manager, Roles.Admin));
    options.AddPolicy(Policies.CanDeleteOwnDocuments, policy => policy.RequireRole(Roles.Contributor, Roles.Manager, Roles.Admin));
    options.AddPolicy(Policies.CanManageTeamDocuments, policy => policy.RequireRole(Roles.Manager, Roles.Admin));
    options.AddPolicy(Policies.CanViewAuditLogs, policy => policy.RequireRole(Roles.Admin));
    options.AddPolicy(Policies.CanManageUsers, policy => policy.RequireRole(Roles.Admin));
});

// Register authorization handlers
builder.Services.AddSingleton<Microsoft.AspNetCore.Authorization.IAuthorizationHandler, RoleAuthorizationHandler>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Enterprise Document Management System API", 
        Version = "v1",
        Description = "API for managing enterprise documents with role-based access control"
    });
    
    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter your token in the text input below (without 'Bearer' prefix).",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // Support for file uploads in Swagger
    c.MapType<IFormFile>(() => new OpenApiSchema
    {
        Type = "string",
        Format = "binary"
    });
});

var app = builder.Build();

// Add global exception handling
app.UseExceptionHandling();

// Add Serilog request logging
app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
        diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
        diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].ToString());
        
        if (httpContext.User.Identity?.IsAuthenticated == true)
        {
            diagnosticContext.Set("UserId", httpContext.User.FindFirst("sub")?.Value ?? "Unknown");
            diagnosticContext.Set("UserEmail", httpContext.User.FindFirst("email")?.Value ?? "Unknown");
        }
    };
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Enable CORS - must be before Authentication and Authorization
app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

    Log.Information("API started successfully");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.Information("Shutting down API");
    Log.CloseAndFlush();
}
