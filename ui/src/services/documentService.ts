import { apiClient } from './api';

export interface Document {
  id: string;
  title: string;
  description?: string;
  fileName: string;
  fileSize: number;
  contentType: string;
  accessType: number;
  uploadedBy: string;
  createdDate: string;
  lastModifiedDate: string;
  tags: Tag[];
  shares: DocumentShare[];
  canEdit: boolean;
  canDelete: boolean;
  canShare: boolean;
}

export interface DocumentListItem {
  id: string;
  title: string;
  description?: string;
  fileName: string;
  fileSize: number;
  contentType: string;
  accessType: number;
  uploadedBy: string;
  createdDate: string;
  lastModifiedDate: string;
  tagNames: string[];
  isShared: boolean;
}

export interface Tag {
  id: string;
  name: string;
  color?: string;
}

export interface DocumentShare {
  id: string;
  sharedWithUserId: string;
  permissionLevel: number;
  sharedBy: string;
  sharedDate: string;
  expirationDate?: string;
  isRevoked: boolean;
}

export interface SearchRequest {
  searchTerm?: string;
  tags?: string[];
  accessType?: number;
  contentType?: string;
  pageNumber: number;
  pageSize: number;
}

export interface PaginatedResponse<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface SearchResponse {
  documents: DocumentListItem[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

export interface DocumentUploadRequest {
  file: File;
  title: string;
  description?: string;
  accessType: string;
  tags?: string;
}

export const documentService = {
  // Get user's documents
  getUserDocuments: async (pageNumber: number = 1, pageSize: number = 20): Promise<PaginatedResponse<DocumentListItem>> => {
    const response = await apiClient.get<PaginatedResponse<DocumentListItem>>('/documents/my-documents', {
      params: { pageNumber, pageSize }
    });
    return response.data;
  },

  // Alias for backward compatibility
  getMyDocuments: async (pageNumber: number = 1, pageSize: number = 20): Promise<PaginatedResponse<DocumentListItem>> => {
    const response = await apiClient.get<PaginatedResponse<DocumentListItem>>('/documents/my-documents', {
      params: { pageNumber, pageSize }
    });
    return response.data;
  },

  // Get shared documents
  getSharedDocuments: async (pageNumber: number = 1, pageSize: number = 20): Promise<PaginatedResponse<DocumentListItem>> => {
    const response = await apiClient.get<PaginatedResponse<DocumentListItem>>('/documents/shared-with-me', {
      params: { pageNumber, pageSize }
    });
    return response.data;
  },

  // Get public documents
  getPublicDocuments: async (pageNumber: number = 1, pageSize: number = 20): Promise<PaginatedResponse<DocumentListItem>> => {
    const response = await apiClient.get<PaginatedResponse<DocumentListItem>>('/documents/public', {
      params: { pageNumber, pageSize }
    });
    return response.data;
  },

  // Search documents
  searchDocuments: async (request: SearchRequest): Promise<SearchResponse> => {
    const response = await apiClient.post<SearchResponse>('/documents/search', request);
    return response.data;
  },

  // Get document by ID
  getDocument: async (id: string): Promise<Document> => {
    const response = await apiClient.get<Document>(`/documents/${id}`);
    return response.data;
  },

  // Upload document
  uploadDocument: async (request: DocumentUploadRequest): Promise<Document> => {
    const formData = new FormData();
    formData.append('file', request.file);
    formData.append('title', request.title);
    if (request.description) {
      formData.append('description', request.description);
    }
    formData.append('accessType', request.accessType);
    if (request.tags) {
      formData.append('tags', request.tags);
    }

    const response = await apiClient.post<Document>('/documents/upload', formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
    return response.data;
  },

  // Update document metadata
  updateDocument: async (id: string, data: {
    title?: string;
    description?: string;
    accessType?: number;
    tags?: string[];
  }): Promise<Document> => {
    const response = await apiClient.put<Document>(`/documents/${id}`, data);
    return response.data;
  },

  // Delete document
  deleteDocument: async (id: string): Promise<void> => {
    await apiClient.delete(`/documents/${id}`);
  },

  // Download document
  downloadDocument: async (id: string, fileName: string): Promise<void> => {
    const response = await apiClient.get(`/documents/${id}/download`, {
      responseType: 'blob',
    });
    
    // Create download link
    const url = window.URL.createObjectURL(new Blob([response.data]));
    const link = document.createElement('a');
    link.href = url;
    link.setAttribute('download', fileName);
    document.body.appendChild(link);
    link.click();
    link.remove();
    window.URL.revokeObjectURL(url);
  },

  // Get all tags
  getTags: async (): Promise<Tag[]> => {
    const response = await apiClient.get<Tag[]>('/tags');
    return response.data;
  },

  // Get popular tags
  getPopularTags: async (count: number = 10): Promise<Tag[]> => {
    const response = await apiClient.get<Tag[]>(`/tags/popular?count=${count}`);
    return response.data;
  },
};
