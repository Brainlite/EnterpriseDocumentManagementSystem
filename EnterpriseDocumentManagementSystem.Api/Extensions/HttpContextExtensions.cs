using EnterpriseDocumentManagementSystem.Api.Models;

namespace EnterpriseDocumentManagementSystem.Api.Extensions;

/// <summary>
/// Extension methods for HttpContext to easily access current user
/// </summary>
public static class HttpContextExtensions
{
    private const string CurrentUserKey = "CurrentUser";

    /// <summary>
    /// Gets the current authenticated user from HttpContext
    /// </summary>
    public static UserPayload? GetCurrentUser(this HttpContext context)
    {
        return context.Items[CurrentUserKey] as UserPayload;
    }

    /// <summary>
    /// Gets the current authenticated user or throws if not found
    /// </summary>
    public static UserPayload GetCurrentUserOrThrow(this HttpContext context)
    {
        var user = context.GetCurrentUser();
        if (user == null)
        {
            throw new UnauthorizedAccessException("User is not authenticated");
        }
        return user;
    }

    /// <summary>
    /// Gets the current user's ID
    /// </summary>
    public static string? GetCurrentUserId(this HttpContext context)
    {
        return context.GetCurrentUser()?.Sub;
    }

    /// <summary>
    /// Gets the current user's role
    /// </summary>
    public static string? GetCurrentUserRole(this HttpContext context)
    {
        return context.GetCurrentUser()?.Role;
    }

    /// <summary>
    /// Checks if the current user has a specific role
    /// </summary>
    public static bool HasRole(this HttpContext context, string role)
    {
        return context.GetCurrentUserRole()?.Equals(role, StringComparison.OrdinalIgnoreCase) ?? false;
    }

    /// <summary>
    /// Checks if the current user has any of the specified roles
    /// </summary>
    public static bool HasAnyRole(this HttpContext context, params string[] roles)
    {
        var userRole = context.GetCurrentUserRole();
        return userRole != null && roles.Any(r => r.Equals(userRole, StringComparison.OrdinalIgnoreCase));
    }
}
