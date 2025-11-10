namespace EnterpriseDocumentManagementSystem.Api.Models;

/// <summary>
/// User roles in the system with hierarchical permissions
/// </summary>
public enum UserRole
{
    /// <summary>
    /// Can view public and shared documents
    /// </summary>
    Viewer = 0,

    /// <summary>
    /// Can create, edit, and delete their own documents
    /// </summary>
    Contributor = 1,

    /// <summary>
    /// Can manage documents from their team, assign access
    /// </summary>
    Manager = 2,

    /// <summary>
    /// Full system access, user management, audit logs
    /// </summary>
    Admin = 3
}

/// <summary>
/// Role names as constants for use in authorization attributes
/// </summary>
public static class Roles
{
    public const string Viewer = nameof(UserRole.Viewer);
    public const string Contributor = nameof(UserRole.Contributor);
    public const string Manager = nameof(UserRole.Manager);
    public const string Admin = nameof(UserRole.Admin);

    // Combined roles for convenience
    public const string ContributorOrHigher = Contributor + "," + Manager + "," + Admin;
    public const string ManagerOrHigher = Manager + "," + Admin;
    public const string AdminOnly = Admin;

    public static readonly string[] AllRoles = { Viewer, Contributor, Manager, Admin };

    public static bool IsValidRole(string role)
    {
        return AllRoles.Contains(role, StringComparer.OrdinalIgnoreCase);
    }

    public static UserRole ParseRole(string role)
    {
        return Enum.Parse<UserRole>(role, ignoreCase: true);
    }

    public static bool HasPermission(string userRole, string requiredRole)
    {
        if (!IsValidRole(userRole) || !IsValidRole(requiredRole))
            return false;

        var userRoleEnum = ParseRole(userRole);
        var requiredRoleEnum = ParseRole(requiredRole);

        // Higher role values have all permissions of lower roles
        return userRoleEnum >= requiredRoleEnum;
    }
}
