using EnterpriseDocumentManagementSystem.Api.Models;
using EnterpriseDocumentManagementSystem.Api.Models.Entities;

namespace EnterpriseDocumentManagementSystem.Api.Services;

public class DocumentAuthorizationService : IAuthorizationService
{
    public bool CanViewDocument(string userRole, string userId, Document document)
    {
        var role = Roles.ParseRole(userRole);

        // Admin and Manager can view all documents
        if (role >= UserRole.Manager)
            return true;

        // Owner can always view their own documents
        if (document.UploadedBy == userId)
            return true;

        // Public documents can be viewed by all authenticated users (Viewer and above)
        if (document.AccessType == AccessType.Public)
            return true;

        // For restricted/private documents, check if shared
        // This will be checked separately in the service layer
        return false;
    }

    public bool CanEditDocument(string userRole, string userId, Document document)
    {
        var role = Roles.ParseRole(userRole);

        // Admin can edit all documents
        if (role == UserRole.Admin)
            return true;

        // Manager can edit team documents (for now, all non-admin documents)
        if (role == UserRole.Manager)
            return true;

        // Contributors can edit their own documents
        if (role >= UserRole.Contributor && document.UploadedBy == userId)
            return true;

        // Viewers cannot edit
        return false;
    }

    public bool CanDeleteDocument(string userRole, string userId, Document document)
    {
        var role = Roles.ParseRole(userRole);

        // Admin can delete all documents
        if (role == UserRole.Admin)
            return true;

        // Manager can delete team documents
        if (role == UserRole.Manager)
            return true;

        // Contributors can delete their own documents
        if (role >= UserRole.Contributor && document.UploadedBy == userId)
            return true;

        // Viewers cannot delete
        return false;
    }

    public bool CanShareDocument(string userRole, string userId, Document document)
    {
        var role = Roles.ParseRole(userRole);

        // Admin can share all documents
        if (role == UserRole.Admin)
            return true;

        // Manager can share team documents
        if (role == UserRole.Manager)
            return true;

        // Contributors can share their own documents
        if (role >= UserRole.Contributor && document.UploadedBy == userId)
            return true;

        // Viewers cannot share
        return false;
    }

    public bool CanViewAuditLogs(string userRole)
    {
        var role = Roles.ParseRole(userRole);
        
        // Only Admin can view audit logs
        return role == UserRole.Admin;
    }

    public bool CanManageUsers(string userRole)
    {
        var role = Roles.ParseRole(userRole);
        
        // Only Admin can manage users
        return role == UserRole.Admin;
    }

    public bool CanCreateDocuments(string userRole)
    {
        var role = Roles.ParseRole(userRole);
        
        // Contributors and above can create documents
        return role >= UserRole.Contributor;
    }
}
