namespace EnterpriseDocumentManagementSystem.Api.Models;


public record LoginRequest(string Email, string Password);

public record LoginResponse(string Token, UserPayload User);

public record UserPayload(string Sub, string Email, string Role, string Name);

// Internal model - using class with init-only properties
public class User
{
    public required string Id { get; init; }
    public required string Email { get; init; }
    public required string Password { get; init; }
    public required string Role { get; init; }
    public required string Name { get; init; }
}

public static class UserRoles
{
    public const string Viewer = "Viewer";
    public const string Contributor = "Contributor";
    public const string Manager = "Manager";
    public const string Admin = "Admin";
}
