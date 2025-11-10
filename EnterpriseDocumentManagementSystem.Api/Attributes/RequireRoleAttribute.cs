using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using EnterpriseDocumentManagementSystem.Api.Extensions;

namespace EnterpriseDocumentManagementSystem.Api.Attributes;

/// <summary>
/// Attribute that requires the user to have one of the specified roles
/// Automatically validates JWT token and extracts user information
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class RequireRoleAttribute : Attribute, IAsyncActionFilter
{
    private readonly string[] _allowedRoles;

    public RequireRoleAttribute(params string[] allowedRoles)
    {
        _allowedRoles = allowedRoles ?? throw new ArgumentNullException(nameof(allowedRoles));
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var user = context.HttpContext.GetCurrentUser();

        if (user == null)
        {
            context.Result = new UnauthorizedObjectResult(new { message = "Authentication required" });
            return;
        }

        if (!_allowedRoles.Any(role => role.Equals(user.Role, StringComparison.OrdinalIgnoreCase)))
        {
            context.Result = new ForbidResult();
            return;
        }

        await next();
    }
}
