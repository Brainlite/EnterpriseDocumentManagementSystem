using Microsoft.AspNetCore.Authorization;
using EnterpriseDocumentManagementSystem.Api.Models;

namespace EnterpriseDocumentManagementSystem.Api.Authorization;

public class RoleAuthorizationHandler : AuthorizationHandler<RoleRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        RoleRequirement requirement)
    {
        var roleClaim = context.User.FindFirst("role") ?? context.User.FindFirst("http://schemas.microsoft.com/ws/2008/06/identity/claims/role");
        
        if (roleClaim == null)
        {
            return Task.CompletedTask;
        }

        var userRole = roleClaim.Value;

        if (requirement.AllowHigherRoles)
        {
            // Check if user has required role or higher
            if (Roles.HasPermission(userRole, requirement.RequiredRole))
            {
                context.Succeed(requirement);
            }
        }
        else
        {
            // Exact role match required
            if (string.Equals(userRole, requirement.RequiredRole, StringComparison.OrdinalIgnoreCase))
            {
                context.Succeed(requirement);
            }
        }

        return Task.CompletedTask;
    }
}
