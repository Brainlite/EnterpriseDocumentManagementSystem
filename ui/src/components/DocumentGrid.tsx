import { DataGrid } from '@mui/x-data-grid';
import type { GridColDef, GridRenderCellParams } from '@mui/x-data-grid';
import { Box, Chip, IconButton, Tooltip } from '@mui/material';
import { ViewIcon, DownloadIcon, DeleteIcon, FileIcon, EditIcon } from './Icons';
import type { DocumentListItem } from '../services/documentService';

interface DocumentGridProps {
  documents: DocumentListItem[];
  loading: boolean;
  onView: (id: string) => void;
  onDownload: (id: string, fileName: string) => void;
  onEdit: (document: DocumentListItem) => void;
  onDelete: (id: string, title: string) => void;
  currentUserEmail: string;
  // Pagination props
  totalCount?: number;
  pageNumber?: number;
  pageSize?: number;
  onPageChange?: (page: number) => void;
  onPageSizeChange?: (pageSize: number) => void;
}

export const DocumentGrid = ({
  documents,
  loading,
  onView,
  onDownload,
  onEdit,
  onDelete,
  currentUserEmail,
  totalCount = 0,
  pageNumber = 1,
  pageSize = 20,
  onPageChange,
  onPageSizeChange,
}: DocumentGridProps) => {
  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
    });
  };

  const formatFileSize = (bytes: number) => {
    if (bytes < 1024) return bytes + ' B';
    if (bytes < 1024 * 1024) return (bytes / 1024).toFixed(1) + ' KB';
    return (bytes / (1024 * 1024)).toFixed(1) + ' MB';
  };

  const getAccessTypeBadge = (accessType: number) => {
    switch (accessType) {
      case 0:
        return <Chip label="Public" color="success" size="small" />;
      case 1:
        return <Chip label="Private" color="default" size="small" />;
      case 2:
        return <Chip label="Restricted" color="warning" size="small" />;
      default:
        return <Chip label="Unknown" size="small" />;
    }
  };

  const columns: GridColDef[] = [
    {
      field: 'title',
      headerName: 'Title',
      flex: 1,
      minWidth: 200,
      renderCell: (params: GridRenderCellParams) => (
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <FileIcon size={16} />
          <Box>
            <Box sx={{ fontWeight: 600 }}>{params.row.title}</Box>
            {params.row.description && (
              <Box sx={{ fontSize: '0.75rem', color: 'text.secondary' }}>
                {params.row.description}
              </Box>
            )}
          </Box>
        </Box>
      ),
    },
    {
      field: 'fileName',
      headerName: 'File Name',
      width: 180,
    },
    {
      field: 'fileSize',
      headerName: 'Size',
      width: 100,
      valueFormatter: (value) => formatFileSize(value as number),
    },
    {
      field: 'accessType',
      headerName: 'Access',
      width: 120,
      renderCell: (params: GridRenderCellParams) => getAccessTypeBadge(params.value as number),
    },
    {
      field: 'tagNames',
      headerName: 'Tags',
      flex: 1,
      minWidth: 150,
      renderCell: (params: GridRenderCellParams) => {
        const tags = params.value as string[];
        if (!tags || tags.length === 0) return <span style={{ color: '#9ca3af' }}>No tags</span>;
        
        return (
          <Box sx={{ display: 'flex', gap: 0.5, flexWrap: 'wrap' }}>
            {tags.slice(0, 2).map((tag, index) => (
              <Chip key={index} label={tag} size="small" variant="outlined" />
            ))}
            {tags.length > 2 && (
              <Chip label={`+${tags.length - 2}`} size="small" variant="outlined" />
            )}
          </Box>
        );
      },
    },
    {
      field: 'createdDate',
      headerName: 'Created',
      width: 120,
      valueFormatter: (value) => formatDate(value as string),
    },
    {
      field: 'actions',
      headerName: 'Actions',
      width: 200,
      sortable: false,
      renderCell: (params: GridRenderCellParams) => (
        <Box sx={{ display: 'flex', gap: 0.5 }}>
          <Tooltip title="View">
            <IconButton size="small" onClick={() => onView(params.row.id)}>
              <ViewIcon />
            </IconButton>
          </Tooltip>
          <Tooltip title="Download">
            <IconButton size="small" onClick={() => onDownload(params.row.id, params.row.fileName)}>
              <DownloadIcon />
            </IconButton>
          </Tooltip>
          {params.row.uploadedBy === currentUserEmail && (
            <>
              <Tooltip title="Edit">
                <IconButton
                  size="small"
                  color="primary"
                  onClick={() => onEdit(params.row as any)}
                >
                  <EditIcon />
                </IconButton>
              </Tooltip>
              <Tooltip title="Delete">
                <IconButton
                  size="small"
                  color="error"
                  onClick={() => onDelete(params.row.id, params.row.title)}
                >
                  <DeleteIcon />
                </IconButton>
              </Tooltip>
            </>
          )}
        </Box>
      ),
    },
  ];

  return (
    <Box sx={{ height: 600, width: '100%' }}>
      <DataGrid
        rows={documents}
        columns={columns}
        loading={loading}
        // Server-side pagination
        paginationMode={onPageChange ? 'server' : 'client'}
        rowCount={totalCount}
        paginationModel={{
          page: pageNumber - 1, // MUI uses 0-indexed pages
          pageSize: pageSize,
        }}
        onPaginationModelChange={(model) => {
          if (onPageChange && model.page !== pageNumber - 1) {
            onPageChange(model.page + 1); // Convert back to 1-indexed
          }
          if (onPageSizeChange && model.pageSize !== pageSize) {
            onPageSizeChange(model.pageSize);
          }
        }}
        pageSizeOptions={[20, 50, 100]}
        disableRowSelectionOnClick
        sx={{
          '& .MuiDataGrid-cell': {
            padding: '8px',
          },
          '& .MuiDataGrid-columnHeaders': {
            backgroundColor: '#f9fafb',
            fontWeight: 600,
          },
        }}
      />
    </Box>
  );
};
