import { describe, it, expect, vi, beforeEach } from 'vitest';
import { documentService, type DocumentUploadRequest, type SearchRequest } from '../documentService';
import { apiClient } from '../api';

vi.mock('../api');

describe('documentService', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe('getUserDocuments', () => {
    it('should fetch user documents with pagination', async () => {
      // Arrange
      const mockResponse = {
        items: [
          {
            id: '1',
            title: 'Test Document',
            fileName: 'test.pdf',
            fileSize: 1024,
            contentType: 'application/pdf',
            accessType: 0,
            uploadedBy: 'user1',
            createdDate: '2024-01-01',
            lastModifiedDate: '2024-01-01',
            tagNames: ['tag1'],
            isShared: false,
          },
        ],
        totalCount: 1,
        pageNumber: 1,
        pageSize: 20,
        totalPages: 1,
        hasPreviousPage: false,
        hasNextPage: false,
      };

      vi.mocked(apiClient.get).mockResolvedValue({ data: mockResponse });

      // Act
      const result = await documentService.getUserDocuments(1, 20);

      // Assert
      expect(apiClient.get).toHaveBeenCalledWith('/documents/my-documents', {
        params: { pageNumber: 1, pageSize: 20 },
      });
      expect(result).toEqual(mockResponse);
      expect(result.items).toHaveLength(1);
    });
  });

  describe('getSharedDocuments', () => {
    it('should fetch shared documents', async () => {
      // Arrange
      const mockResponse = {
        items: [],
        totalCount: 0,
        pageNumber: 1,
        pageSize: 20,
        totalPages: 0,
        hasPreviousPage: false,
        hasNextPage: false,
      };

      vi.mocked(apiClient.get).mockResolvedValue({ data: mockResponse });

      // Act
      const result = await documentService.getSharedDocuments();

      // Assert
      expect(apiClient.get).toHaveBeenCalledWith('/documents/shared-with-me', {
        params: { pageNumber: 1, pageSize: 20 },
      });
      expect(result).toEqual(mockResponse);
    });
  });

  describe('getPublicDocuments', () => {
    it('should fetch public documents', async () => {
      // Arrange
      const mockResponse = {
        items: [],
        totalCount: 0,
        pageNumber: 1,
        pageSize: 20,
        totalPages: 0,
        hasPreviousPage: false,
        hasNextPage: false,
      };

      vi.mocked(apiClient.get).mockResolvedValue({ data: mockResponse });

      // Act
      const result = await documentService.getPublicDocuments();

      // Assert
      expect(apiClient.get).toHaveBeenCalledWith('/documents/public', {
        params: { pageNumber: 1, pageSize: 20 },
      });
      expect(result).toEqual(mockResponse);
    });
  });

  describe('searchDocuments', () => {
    it('should search documents with filters', async () => {
      // Arrange
      const searchRequest: SearchRequest = {
        searchTerm: 'test',
        tags: ['tag1'],
        pageNumber: 1,
        pageSize: 10,
      };

      const mockResponse = {
        documents: [],
        totalCount: 0,
        pageNumber: 1,
        pageSize: 10,
        totalPages: 0,
      };

      vi.mocked(apiClient.post).mockResolvedValue({ data: mockResponse });

      // Act
      const result = await documentService.searchDocuments(searchRequest);

      // Assert
      expect(apiClient.post).toHaveBeenCalledWith('/documents/search', searchRequest);
      expect(result).toEqual(mockResponse);
    });
  });

  describe('getDocument', () => {
    it('should fetch document by ID', async () => {
      // Arrange
      const documentId = '123';
      const mockDocument = {
        id: documentId,
        title: 'Test Document',
        fileName: 'test.pdf',
        fileSize: 1024,
        contentType: 'application/pdf',
        accessType: 0,
        uploadedBy: 'user1',
        createdDate: '2024-01-01',
        lastModifiedDate: '2024-01-01',
        tags: [],
        shares: [],
        canEdit: true,
        canDelete: true,
        canShare: true,
      };

      vi.mocked(apiClient.get).mockResolvedValue({ data: mockDocument });

      // Act
      const result = await documentService.getDocument(documentId);

      // Assert
      expect(apiClient.get).toHaveBeenCalledWith(`/documents/${documentId}`);
      expect(result).toEqual(mockDocument);
    });
  });

  describe('uploadDocument', () => {
    it('should upload document with metadata', async () => {
      // Arrange
      const file = new File(['content'], 'test.pdf', { type: 'application/pdf' });
      const request: DocumentUploadRequest = {
        file,
        title: 'Test Document',
        description: 'Test description',
        accessType: 'Public',
        tags: 'tag1,tag2',
      };

      const mockResponse = {
        id: '1',
        title: 'Test Document',
        fileName: 'test.pdf',
        fileSize: 1024,
        contentType: 'application/pdf',
        accessType: 0,
        uploadedBy: 'user1',
        createdDate: '2024-01-01',
        lastModifiedDate: '2024-01-01',
        tags: [],
        shares: [],
        canEdit: true,
        canDelete: true,
        canShare: true,
      };

      vi.mocked(apiClient.post).mockResolvedValue({ data: mockResponse });

      // Act
      const result = await documentService.uploadDocument(request);

      // Assert
      expect(apiClient.post).toHaveBeenCalledWith(
        '/documents/upload',
        expect.any(FormData),
        { headers: { 'Content-Type': 'multipart/form-data' } }
      );
      expect(result).toEqual(mockResponse);
    });
  });

  describe('updateDocument', () => {
    it('should update document metadata', async () => {
      // Arrange
      const documentId = '123';
      const updateData = {
        title: 'Updated Title',
        description: 'Updated description',
        accessType: 1,
        tags: ['tag1', 'tag2'],
      };

      const mockResponse = {
        id: documentId,
        title: 'Updated Title',
        fileName: 'test.pdf',
        fileSize: 1024,
        contentType: 'application/pdf',
        accessType: 1,
        uploadedBy: 'user1',
        createdDate: '2024-01-01',
        lastModifiedDate: '2024-01-02',
        tags: [],
        shares: [],
        canEdit: true,
        canDelete: true,
        canShare: true,
      };

      vi.mocked(apiClient.put).mockResolvedValue({ data: mockResponse });

      // Act
      const result = await documentService.updateDocument(documentId, updateData);

      // Assert
      expect(apiClient.put).toHaveBeenCalledWith(`/documents/${documentId}`, updateData);
      expect(result).toEqual(mockResponse);
    });
  });

  describe('deleteDocument', () => {
    it('should delete document by ID', async () => {
      // Arrange
      const documentId = '123';
      vi.mocked(apiClient.delete).mockResolvedValue({ data: {} });

      // Act
      await documentService.deleteDocument(documentId);

      // Assert
      expect(apiClient.delete).toHaveBeenCalledWith(`/documents/${documentId}`);
    });
  });

  describe('getTags', () => {
    it('should fetch all tags', async () => {
      // Arrange
      const mockTags = [
        { id: '1', name: 'Tag1', color: '#FF0000' },
        { id: '2', name: 'Tag2', color: '#00FF00' },
      ];

      vi.mocked(apiClient.get).mockResolvedValue({ data: mockTags });

      // Act
      const result = await documentService.getTags();

      // Assert
      expect(apiClient.get).toHaveBeenCalledWith('/tags');
      expect(result).toEqual(mockTags);
      expect(result).toHaveLength(2);
    });
  });

  describe('getPopularTags', () => {
    it('should fetch popular tags with default count', async () => {
      // Arrange
      const mockTags = [
        { id: '1', name: 'Popular1' },
        { id: '2', name: 'Popular2' },
      ];

      vi.mocked(apiClient.get).mockResolvedValue({ data: mockTags });

      // Act
      const result = await documentService.getPopularTags();

      // Assert
      expect(apiClient.get).toHaveBeenCalledWith('/tags/popular?count=10');
      expect(result).toEqual(mockTags);
    });

    it('should fetch popular tags with custom count', async () => {
      // Arrange
      const mockTags = [{ id: '1', name: 'Popular1' }];
      vi.mocked(apiClient.get).mockResolvedValue({ data: mockTags });

      // Act
      const result = await documentService.getPopularTags(5);

      // Assert
      expect(apiClient.get).toHaveBeenCalledWith('/tags/popular?count=5');
      expect(result).toEqual(mockTags);
    });
  });
});
