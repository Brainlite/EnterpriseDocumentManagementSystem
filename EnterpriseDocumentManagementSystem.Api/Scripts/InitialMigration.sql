-- =============================================
-- Enterprise Document Management System
-- Initial Database Schema
-- =============================================

-- Create Database (if not exists)
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'EnterpriseDocumentManagementDB')
BEGIN
    CREATE DATABASE EnterpriseDocumentManagementDB;
END
GO

USE EnterpriseDocumentManagementDB;
GO

-- =============================================
-- Drop existing tables (for clean setup)
-- =============================================
IF OBJECT_ID('dbo.AuditLogs', 'U') IS NOT NULL DROP TABLE dbo.AuditLogs;
IF OBJECT_ID('dbo.DocumentTags', 'U') IS NOT NULL DROP TABLE dbo.DocumentTags;
IF OBJECT_ID('dbo.DocumentShares', 'U') IS NOT NULL DROP TABLE dbo.DocumentShares;
IF OBJECT_ID('dbo.Tags', 'U') IS NOT NULL DROP TABLE dbo.Tags;
IF OBJECT_ID('dbo.Documents', 'U') IS NOT NULL DROP TABLE dbo.Documents;
GO

-- =============================================
-- Create Documents Table
-- =============================================
CREATE TABLE dbo.Documents (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Title NVARCHAR(255) NOT NULL,
    Description NVARCHAR(2000) NULL,
    FileName NVARCHAR(500) NOT NULL,
    FileSize BIGINT NOT NULL CHECK (FileSize > 0),
    ContentType NVARCHAR(100) NOT NULL,
    FilePath NVARCHAR(500) NULL,
    AccessType INT NOT NULL CHECK (AccessType IN (0, 1, 2)), -- 0=Public, 1=Private, 2=Restricted
    UploadedBy NVARCHAR(100) NOT NULL,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    LastModifiedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedDate DATETIME2 NULL
);
GO

-- Create indexes on Documents table
CREATE NONCLUSTERED INDEX IX_Documents_UploadedBy ON dbo.Documents(UploadedBy);
CREATE NONCLUSTERED INDEX IX_Documents_CreatedDate ON dbo.Documents(CreatedDate DESC);
CREATE NONCLUSTERED INDEX IX_Documents_AccessType ON dbo.Documents(AccessType);
CREATE NONCLUSTERED INDEX IX_Documents_IsDeleted ON dbo.Documents(IsDeleted);
CREATE NONCLUSTERED INDEX IX_Documents_UploadedBy_IsDeleted ON dbo.Documents(UploadedBy, IsDeleted);
GO

-- =============================================
-- Create Tags Table
-- =============================================
CREATE TABLE dbo.Tags (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(50) NOT NULL,
    Color NVARCHAR(100) NULL,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(100) NOT NULL,
    CONSTRAINT UQ_Tags_Name UNIQUE (Name)
);
GO

-- Create index on Tags table
CREATE UNIQUE NONCLUSTERED INDEX IX_Tags_Name_Unique ON dbo.Tags(Name);
GO

-- =============================================
-- Create DocumentShares Table
-- =============================================
CREATE TABLE dbo.DocumentShares (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    DocumentId UNIQUEIDENTIFIER NOT NULL,
    SharedWithUserId NVARCHAR(100) NOT NULL,
    PermissionLevel INT NOT NULL CHECK (PermissionLevel IN (0, 1, 2)), -- 0=View, 1=Edit, 2=FullControl
    SharedBy NVARCHAR(100) NOT NULL,
    SharedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ExpirationDate DATETIME2 NULL,
    IsRevoked BIT NOT NULL DEFAULT 0,
    RevokedDate DATETIME2 NULL,
    RevokedBy NVARCHAR(100) NULL,
    CONSTRAINT FK_DocumentShares_Documents FOREIGN KEY (DocumentId) 
        REFERENCES dbo.Documents(Id) ON DELETE CASCADE
);
GO

-- Create indexes on DocumentShares table
CREATE NONCLUSTERED INDEX IX_DocumentShares_DocumentId ON dbo.DocumentShares(DocumentId);
CREATE NONCLUSTERED INDEX IX_DocumentShares_SharedWithUserId ON dbo.DocumentShares(SharedWithUserId);
CREATE NONCLUSTERED INDEX IX_DocumentShares_DocumentId_SharedWithUserId_IsRevoked 
    ON dbo.DocumentShares(DocumentId, SharedWithUserId, IsRevoked);
GO

-- =============================================
-- Create DocumentTags Table (Many-to-Many)
-- =============================================
CREATE TABLE dbo.DocumentTags (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    DocumentId UNIQUEIDENTIFIER NOT NULL,
    TagId UNIQUEIDENTIFIER NOT NULL,
    AssignedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    AssignedBy NVARCHAR(100) NOT NULL,
    CONSTRAINT FK_DocumentTags_Documents FOREIGN KEY (DocumentId) 
        REFERENCES dbo.Documents(Id) ON DELETE CASCADE,
    CONSTRAINT FK_DocumentTags_Tags FOREIGN KEY (TagId) 
        REFERENCES dbo.Tags(Id) ON DELETE CASCADE,
    CONSTRAINT UQ_DocumentTags_DocumentId_TagId UNIQUE (DocumentId, TagId)
);
GO

-- Create indexes on DocumentTags table
CREATE NONCLUSTERED INDEX IX_DocumentTags_DocumentId ON dbo.DocumentTags(DocumentId);
CREATE NONCLUSTERED INDEX IX_DocumentTags_TagId ON dbo.DocumentTags(TagId);
CREATE UNIQUE NONCLUSTERED INDEX IX_DocumentTags_DocumentId_TagId_Unique 
    ON dbo.DocumentTags(DocumentId, TagId);
GO

-- =============================================
-- Create AuditLogs Table
-- =============================================
CREATE TABLE dbo.AuditLogs (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    DocumentId UNIQUEIDENTIFIER NULL,
    UserId NVARCHAR(100) NOT NULL,
    Action NVARCHAR(100) NOT NULL,
    ActionType INT NOT NULL CHECK (ActionType IN (0, 1, 2, 3, 4, 5, 6, 7, 8)), 
    -- 0=Create, 1=Read, 2=Update, 3=Delete, 4=Share, 5=Download, 6=Login, 7=Logout, 8=AccessDenied
    EntityType NVARCHAR(500) NULL,
    EntityId UNIQUEIDENTIFIER NULL,
    Details NVARCHAR(2000) NULL,
    IpAddress NVARCHAR(50) NULL,
    UserAgent NVARCHAR(500) NULL,
    Timestamp DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    IsSuccessful BIT NOT NULL DEFAULT 1,
    ErrorMessage NVARCHAR(1000) NULL,
    CONSTRAINT FK_AuditLogs_Documents FOREIGN KEY (DocumentId) 
        REFERENCES dbo.Documents(Id) ON DELETE SET NULL
);
GO

-- Create indexes on AuditLogs table
CREATE NONCLUSTERED INDEX IX_AuditLogs_DocumentId ON dbo.AuditLogs(DocumentId);
CREATE NONCLUSTERED INDEX IX_AuditLogs_UserId ON dbo.AuditLogs(UserId);
CREATE NONCLUSTERED INDEX IX_AuditLogs_Timestamp ON dbo.AuditLogs(Timestamp DESC);
CREATE NONCLUSTERED INDEX IX_AuditLogs_ActionType ON dbo.AuditLogs(ActionType);
CREATE NONCLUSTERED INDEX IX_AuditLogs_UserId_Timestamp ON dbo.AuditLogs(UserId, Timestamp DESC);
GO

-- =============================================
-- Insert Sample Data (Optional)
-- =============================================

-- Insert sample tags
INSERT INTO dbo.Tags (Id, Name, Color, CreatedBy)
VALUES 
    (NEWID(), 'Important', '#FF0000', 'system'),
    (NEWID(), 'Confidential', '#FFA500', 'system'),
    (NEWID(), 'Draft', '#808080', 'system'),
    (NEWID(), 'Approved', '#00FF00', 'system'),
    (NEWID(), 'Archive', '#0000FF', 'system');
GO

PRINT 'Database schema created successfully!';
PRINT 'Tables created: Documents, Tags, DocumentShares, DocumentTags, AuditLogs';
PRINT 'Indexes created for optimal performance';
PRINT 'Sample tags inserted';
GO
