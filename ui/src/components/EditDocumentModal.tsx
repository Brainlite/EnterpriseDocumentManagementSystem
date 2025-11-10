import { useState } from 'react';
import { useUpdateDocument } from '../hooks/useDocuments';
import { CloseIcon } from './Icons';
import { AccessTypeSelect } from './AccessTypeSelect';
import type { DocumentListItem } from '../services/documentService';
import './UploadModal.css';

interface EditDocumentModalProps {
  document: DocumentListItem;
  onClose: () => void;
  onSuccess: () => void;
}

export const EditDocumentModal = ({ document, onClose, onSuccess }: EditDocumentModalProps) => {
  const [title, setTitle] = useState(document.title);
  const [description, setDescription] = useState(document.description || '');
  const [accessType, setAccessType] = useState(getAccessTypeString(document.accessType));
  const [tags, setTags] = useState(document.tagNames.join(', '));
  const [error, setError] = useState<string | null>(null);

  const updateMutation = useUpdateDocument();

  function getAccessTypeString(accessType: number): string {
    switch (accessType) {
      case 0: return 'Public';
      case 1: return 'Private';
      case 2: return 'Restricted';
      default: return 'Private';
    }
  }

  function getAccessTypeNumber(accessTypeStr: string): number {
    switch (accessTypeStr) {
      case 'Public': return 0;
      case 'Private': return 1;
      case 'Restricted': return 2;
      default: return 1;
    }
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!title.trim()) {
      setError('Please enter a title');
      return;
    }

    try {
      setError(null);

      const tagsArray = tags.trim() 
        ? tags.split(',').map(t => t.trim()).filter(t => t.length > 0)
        : [];

      await updateMutation.mutateAsync({
        id: document.id,
        data: {
          title: title.trim(),
          description: description.trim() || undefined,
          accessType: getAccessTypeNumber(accessType),
          tags: tagsArray.length > 0 ? tagsArray : undefined,
        },
      });

      onSuccess();
    } catch (err: any) {
      // Better error handling
      // TODO: apply globally
      let errorMessage = 'Failed to update document';
      
      if (err.response?.data) {
        const data = err.response.data;
        
        // Handle validation errors
        if (data.errors) {
          const errorMessages = Object.entries(data.errors)
            .map(([field, messages]: [string, any]) => {
              const fieldName = field.replace('$.', '').replace('request.', '');
              const msgs = Array.isArray(messages) ? messages : [messages];
              return `${fieldName}: ${msgs.join(', ')}`;
            })
            .join('\n');
          errorMessage = errorMessages || data.title || errorMessage;
        } else if (data.error) {
          errorMessage = data.error;
        } else if (data.title) {
          errorMessage = data.title;
        }
      } else if (err.message) {
        errorMessage = err.message;
      }
      
      setError(errorMessage);
    }
  };

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal" onClick={(e) => e.stopPropagation()}>
        <div className="modal-header">
          <h2>Edit Document</h2>
          <button className="modal-close" onClick={onClose}>
            <CloseIcon />
          </button>
        </div>

        <form onSubmit={handleSubmit} className="modal-body">
          {error && (
            <div className="error-message" style={{ 
              backgroundColor: '#fee', 
              border: '1px solid #fcc', 
              padding: '12px', 
              borderRadius: '4px',
              marginBottom: '16px',
              whiteSpace: 'pre-wrap'
            }}>
              <strong style={{ color: '#c00' }}>Error:</strong>
              <p style={{ margin: '4px 0 0 0', color: '#600' }}>{error}</p>
            </div>
          )}

          <div className="form-group">
            <label htmlFor="title">Title *</label>
            <input
              type="text"
              id="title"
              value={title}
              onChange={(e) => setTitle(e.target.value)}
              maxLength={255}
              required
            />
          </div>

          <div className="form-group">
            <label htmlFor="description">Description</label>
            <textarea
              id="description"
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              maxLength={2000}
              rows={3}
            />
          </div>

          <AccessTypeSelect
            value={accessType}
            onChange={setAccessType}
          />

          <div className="form-group">
            <label htmlFor="tags">Tags</label>
            <input
              type="text"
              id="tags"
              value={tags}
              onChange={(e) => setTags(e.target.value)}
              placeholder="Comma-separated tags (e.g., Important, Contract, 2024)"
            />
          </div>

          <div className="form-group">
            <label>File Name</label>
            <input
              type="text"
              value={document.fileName}
              disabled
              style={{ backgroundColor: '#f3f4f6', cursor: 'not-allowed' }}
            />
            <small>File cannot be changed after upload</small>
          </div>

          <div className="modal-footer">
            <button type="button" className="btn btn-outline" onClick={onClose} disabled={updateMutation.isPending}>
              Cancel
            </button>
            <button type="submit" className="btn btn-primary" disabled={updateMutation.isPending}>
              {updateMutation.isPending ? 'Saving...' : 'Save Changes'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};
