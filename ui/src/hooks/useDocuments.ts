import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { documentService } from '../services/documentService';
import type { DocumentUploadRequest, SearchRequest } from '../services/documentService';

// Query Keys
export const documentKeys = {
  all: ['documents'] as const,
  lists: () => [...documentKeys.all, 'list'] as const,
  list: (filters: string) => [...documentKeys.lists(), filters] as const,
  details: () => [...documentKeys.all, 'detail'] as const,
  detail: (id: string) => [...documentKeys.details(), id] as const,
};

// Get user documents
export const useUserDocuments = (pageNumber: number = 1, pageSize: number = 20) => {
  return useQuery({
    queryKey: documentKeys.list(`user-${pageNumber}-${pageSize}`),
    queryFn: () => documentService.getUserDocuments(pageNumber, pageSize),
  });
};

// Get shared documents
export const useSharedDocuments = (pageNumber: number = 1, pageSize: number = 20) => {
  return useQuery({
    queryKey: documentKeys.list(`shared-${pageNumber}-${pageSize}`),
    queryFn: () => documentService.getSharedDocuments(pageNumber, pageSize),
  });
};

// Get public documents
export const usePublicDocuments = (pageNumber: number = 1, pageSize: number = 20) => {
  return useQuery({
    queryKey: documentKeys.list(`public-${pageNumber}-${pageSize}`),
    queryFn: () => documentService.getPublicDocuments(pageNumber, pageSize),
  });
};

// Search documents
export const useSearchDocuments = (request: SearchRequest, enabled: boolean = true) => {
  return useQuery({
    queryKey: documentKeys.list(`search-${JSON.stringify(request)}`),
    queryFn: () => documentService.searchDocuments(request),
    enabled,
  });
};

// Upload document
export const useUploadDocument = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request: DocumentUploadRequest) => documentService.uploadDocument(request),
    onSuccess: () => {
      // Invalidate all document lists to refetch
      queryClient.invalidateQueries({ queryKey: documentKeys.lists() });
    },
  });
};

// Update document
export const useUpdateDocument = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, data }: { 
      id: string; 
      data: {
        title?: string;
        description?: string;
        accessType?: number;
        tags?: string[];
      }
    }) => documentService.updateDocument(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: documentKeys.lists() });
    },
  });
};

// Delete document
export const useDeleteDocument = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => documentService.deleteDocument(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: documentKeys.lists() });
    },
  });
};

// Download document
export const useDownloadDocument = () => {
  return useMutation({
    mutationFn: ({ id, fileName }: { id: string; fileName: string }) =>
      documentService.downloadDocument(id, fileName),
  });
};
