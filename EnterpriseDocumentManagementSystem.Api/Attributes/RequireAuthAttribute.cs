using Microsoft.AspNetCore.Mvc;

namespace EnterpriseDocumentManagementSystem.Api.Attributes;

/// <summary>
/// Attribute that applies the CurrentUserFilter to extract and validate JWT token
/// Use this instead of [Authorize] when you need access to the current user in the controller
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class RequireAuthAttribute : ServiceFilterAttribute
{
    public RequireAuthAttribute() : base(typeof(Filters.CurrentUserFilter))
    {
    }
}
