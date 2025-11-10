using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using EnterpriseDocumentManagementSystem.Api.Services;

namespace EnterpriseDocumentManagementSystem.Api.Filters;

/// <summary>
/// Action filter that extracts and validates the current user from JWT token and makes it available via HttpContext.Items
/// </summary>
public class CurrentUserFilter : IAsyncActionFilter
{
    private readonly MockAuthService _authService;

    public CurrentUserFilter(MockAuthService authService)
    {
        _authService = authService;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var authHeader = context.HttpContext.Request.Headers["Authorization"].ToString();

        if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            context.Result = new UnauthorizedObjectResult(new { message = "No token provided" });
            return;
        }

        var token = authHeader["Bearer ".Length..].Trim();
        var user = _authService.ValidateToken(token);

        if (user == null)
        {
            context.Result = new UnauthorizedObjectResult(new { message = "Invalid or expired token" });
            return;
        }

        // Store user in HttpContext.Items for access in controllers
        context.HttpContext.Items["CurrentUser"] = user;
        context.HttpContext.Items["UserId"] = user.Email;
        context.HttpContext.Items["UserRole"] = user.Role;

        await next();
    }
}
