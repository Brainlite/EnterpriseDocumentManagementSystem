import { useState, useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import { Tabs, Tab, Box, TextField, InputAdornment, Button, Container } from '@mui/material';
import { useUserDocuments, useSharedDocuments, usePublicDocuments, useSearchDocuments, useDeleteDocument, useDownloadDocument } from '../hooks/useDocuments';
import { UploadModal } from '../components/UploadModal';
import { EditDocumentModal } from '../components/EditDocumentModal';
import { DocumentGrid } from '../components/DocumentGrid';
import { UploadIcon, LogoutIcon, SearchIcon, FileIcon } from '../components/Icons';
import { getApiUrl, API_CONFIG } from '../config/api.config';
import type { DocumentListItem } from '../services/documentService';

const Dashboard = () => {
  const navigate = useNavigate();
  const [searchTerm, setSearchTerm] = useState('');
  const [showUploadModal, setShowUploadModal] = useState(false);
  const [showEditModal, setShowEditModal] = useState(false);
  const [documentToEdit, setDocumentToEdit] = useState<DocumentListItem | null>(null);
  const [activeTab, setActiveTab] = useState<'my' | 'shared' | 'public'>('my');
  const [isSearching, setIsSearching] = useState(false);

  const user = JSON.parse(localStorage.getItem('user') || '{}');
  const userName = user.name || user.email || 'User';
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(20);

  // React Query hooks
  const { data: myDocumentsResponse, isLoading: loadingMy, error: errorMy } = useUserDocuments(currentPage, pageSize);
  const { data: sharedDocumentsResponse, isLoading: loadingShared, error: errorShared } = useSharedDocuments(currentPage, pageSize);
  const { data: publicDocumentsResponse, isLoading: loadingPublic, error: errorPublic } = usePublicDocuments(currentPage, pageSize);
  const { data: searchResults, isLoading: loadingSearch } = useSearchDocuments(
    { searchTerm, pageNumber: currentPage, pageSize },
    isSearching
  );
  const deleteMutation = useDeleteDocument();
  const downloadMutation = useDownloadDocument();

  // Map tab data
  const tabDataMap = useMemo(() => ({
    my: { data: myDocumentsResponse, loading: loadingMy, error: errorMy },
    shared: { data: sharedDocumentsResponse, loading: loadingShared, error: errorShared },
    public: { data: publicDocumentsResponse, loading: loadingPublic, error: errorPublic },
  }), [myDocumentsResponse, loadingMy, errorMy, sharedDocumentsResponse, loadingShared, errorShared, publicDocumentsResponse, loadingPublic, errorPublic]);

  // Determine which data to show
  const currentTabData = tabDataMap[activeTab];
  
  const documents = isSearching && searchResults
    ? searchResults.documents
    : currentTabData.data?.items || [];
  
  const pagination = isSearching && searchResults
    ? { totalCount: searchResults.totalCount, totalPages: searchResults.totalPages, pageNumber: searchResults.pageNumber }
    : currentTabData.data
    ? { totalCount: currentTabData.data.totalCount, totalPages: currentTabData.data.totalPages, pageNumber: currentTabData.data.pageNumber }
    : { totalCount: 0, totalPages: 0, pageNumber: 1 };

  const loading = isSearching ? loadingSearch : currentTabData.loading;
  const error = isSearching ? null : currentTabData.error;

  const handleLogout = () => {
    localStorage.removeItem('authToken');
    localStorage.removeItem('user');
    navigate('/login');
  };

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    if (searchTerm.trim()) {
      setIsSearching(true);
      setCurrentPage(1);
    }
  };

  const handleDelete = async (id: string, title: string) => {
    if (!window.confirm(`Are you sure you want to delete "${title}"?`)) {
      return;
    }

    try {
      await deleteMutation.mutateAsync(id);
    } catch (err: any) {
      alert(err instanceof Error ? err.message : 'Failed to delete document');
    }
  };

  const handleDownload = async (id: string, fileName: string) => {
    try {
      await downloadMutation.mutateAsync({ id, fileName });
    } catch (err: any) {
      alert(err instanceof Error ? err.message : 'Failed to download document');
    }
  };

  const handleEdit = (document: DocumentListItem) => {
    setDocumentToEdit(document);
    setShowEditModal(true);
  };

  const handleView = async (id: string) => {
    try {
      const token = localStorage.getItem('authToken');
      const response = await fetch(getApiUrl(`${API_CONFIG.ENDPOINTS.DOCUMENTS}/${id}/download`), {
        headers: {
          'Authorization': `Bearer ${token}`
        }
      });
      
      if (!response.ok) {
        throw new Error('Failed to fetch document');
      }
      
      const blob = await response.blob();
      const url = window.URL.createObjectURL(blob);
      window.open(url, '_blank');
      
      // Clean up the URL after a delay
      setTimeout(() => window.URL.revokeObjectURL(url), 100);
    } catch (err: any) {
      alert('Failed to view document');
    }
  };


  return (
    <Container maxWidth="xl" sx={{ py: 4 }}>
      {/* Header */}
      <Box sx={{ mb: 4, display: 'flex', justifyContent: 'space-between', alignItems: 'center', flexWrap: 'wrap', gap: 2 }}>
        <Box>
          <h1 style={{ margin: 0, fontSize: '2rem', fontWeight: 700, color: '#111827' }}>Welcome back, {userName}!</h1>
          <p style={{ margin: '0.5rem 0 0 0', color: '#6b7280' }}>Manage your documents efficiently</p>
        </Box>
        <Box sx={{ display: 'flex', gap: 1.5 }}>
          <Button 
            variant="contained"
            startIcon={<UploadIcon />}
            onClick={() => setShowUploadModal(true)}
          >
            Upload Document
          </Button>
          <Button 
            variant="outlined"
            startIcon={<LogoutIcon />}
            onClick={handleLogout}
          >
            Logout
          </Button>
        </Box>
      </Box>

      {/* Tabs */}
      <Box sx={{ borderBottom: 1, borderColor: 'divider', mb: 3 }}>
        <Tabs 
          value={activeTab} 
          onChange={(_, newValue) => { 
            setActiveTab(newValue);
            setIsSearching(false);
            setSearchTerm('');
            setCurrentPage(1);
          }}
        >
          <Tab label="My Documents" value="my" />
          <Tab label="Shared with Me" value="shared" />
          <Tab label="Public Documents" value="public" />
        </Tabs>
      </Box>

      {/* Search Bar */}
      <Box sx={{ mb: 3, display: 'flex', gap: 2 }}>
        <TextField
          fullWidth
          placeholder="Search documents by title, description, or filename..."
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
          onKeyPress={(e) => e.key === 'Enter' && handleSearch(e as any)}
          InputProps={{
            startAdornment: (
              <InputAdornment position="start">
                <SearchIcon />
              </InputAdornment>
            ),
          }}
        />
        <Button variant="contained" onClick={handleSearch}>
          Search
        </Button>
        {searchTerm && (
          <Button
            variant="outlined"
            onClick={() => {
              setSearchTerm('');
              setIsSearching(false);
              setCurrentPage(1);
            }}
          >
            Clear
          </Button>
        )}
      </Box>

      {/* Documents Grid */}
      {error ? (
        <Box sx={{ textAlign: 'center', py: 8 }}>
          <p style={{ color: '#dc2626' }}>{error instanceof Error ? error.message : 'An error occurred'}</p>
        </Box>
      ) : documents.length === 0 && !loading ? (
        <Box sx={{ textAlign: 'center', py: 8 }}>
          <FileIcon size={64} style={{ color: '#9ca3af', marginBottom: '1rem' }} />
          <h3>No documents found</h3>
          <p style={{ color: '#6b7280' }}>
            {searchTerm
              ? 'Try adjusting your search terms'
              : 'Upload your first document to get started'}
          </p>
        </Box>
      ) : (
        <DocumentGrid
          documents={documents}
          loading={loading}
          onView={handleView}
          onDownload={handleDownload}
          onEdit={handleEdit}
          onDelete={handleDelete}
          currentUserEmail={user.email}
          totalCount={pagination.totalCount}
          pageNumber={pagination.pageNumber}
          pageSize={pageSize}
          onPageChange={setCurrentPage}
          onPageSizeChange={(newSize) => {
            setPageSize(newSize);
            setCurrentPage(1); // Reset to first page when changing page size
          }}
        />
      )}

      {/* Upload Modal */}
      {showUploadModal && (
        <UploadModal
          onClose={() => setShowUploadModal(false)}
          onSuccess={() => {
            setShowUploadModal(false);
            // React Query will automatically refetch due to invalidation
          }}
        />
      )}

      {/* Edit Modal */}
      {showEditModal && documentToEdit && (
        <EditDocumentModal
          document={documentToEdit}
          onClose={() => {
            setShowEditModal(false);
            setDocumentToEdit(null);
          }}
          onSuccess={() => {
            setShowEditModal(false);
            setDocumentToEdit(null);
            // React Query will automatically refetch due to invalidation
          }}
        />
      )}
    </Container>
  );
};

export default Dashboard;
