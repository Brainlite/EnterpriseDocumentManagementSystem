using Microsoft.AspNetCore.Authorization;

namespace EnterpriseDocumentManagementSystem.Api.Authorization;

public class RoleRequirement : IAuthorizationRequirement
{
    public string RequiredRole { get; }
    public bool AllowHigherRoles { get; }

    public RoleRequirement(string requiredRole, bool allowHigherRoles = true)
    {
        RequiredRole = requiredRole;
        AllowHigherRoles = allowHigherRoles;
    }
}
