using EnterpriseDocumentManagementSystem.Api.Models.Entities;

namespace EnterpriseDocumentManagementSystem.Api.Services;

public interface IAuthorizationService
{
    bool CanViewDocument(string userRole, string userId, Document document);
    bool CanEditDocument(string userRole, string userId, Document document);
    bool CanDeleteDocument(string userRole, string userId, Document document);
    bool CanShareDocument(string userRole, string userId, Document document);
    bool CanViewAuditLogs(string userRole);
    bool CanManageUsers(string userRole);
    bool CanCreateDocuments(string userRole);
}
