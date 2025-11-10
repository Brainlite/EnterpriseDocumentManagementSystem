namespace EnterpriseDocumentManagementSystem.Api.Authorization;

public static class Policies
{
    // Role-based policies
    public const string ViewerPolicy = "ViewerPolicy";
    public const string ContributorPolicy = "ContributorPolicy";
    public const string ManagerPolicy = "ManagerPolicy";
    public const string AdminPolicy = "AdminPolicy";

    // Feature-based policies
    public const string CanCreateDocuments = "CanCreateDocuments";
    public const string CanEditOwnDocuments = "CanEditOwnDocuments";
    public const string CanDeleteOwnDocuments = "CanDeleteOwnDocuments";
    public const string CanManageTeamDocuments = "CanManageTeamDocuments";
    public const string CanViewAuditLogs = "CanViewAuditLogs";
    public const string CanManageUsers = "CanManageUsers";
}
