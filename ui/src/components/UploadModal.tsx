import { useState } from 'react';
import { useUploadDocument } from '../hooks/useDocuments';
import { CloseIcon } from './Icons';
import { AccessTypeSelect } from './AccessTypeSelect';
import './UploadModal.css';

interface UploadModalProps {
  onClose: () => void;
  onSuccess: () => void;
}

export const UploadModal = ({ onClose, onSuccess }: UploadModalProps) => {
  const [file, setFile] = useState<File | null>(null);
  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [accessType, setAccessType] = useState('Private');
  const [tags, setTags] = useState('');
  const [error, setError] = useState<string | null>(null);

  const uploadMutation = useUploadDocument();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!file) {
      setError('Please select a file');
      return;
    }

    if (!title.trim()) {
      setError('Please enter a title');
      return;
    }

    try {
      setError(null);

      await uploadMutation.mutateAsync({
        file,
        title: title.trim(),
        description: description.trim() || undefined,
        accessType,
        tags: tags.trim() || undefined,
      });

      onSuccess();
    } catch (err: any) {
      // Better error handling
      // TODO: apply globally
      let errorMessage = 'Failed to upload document';
      
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
          <h2>Upload Document</h2>
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
            <label htmlFor="file">File *</label>
            <input
              type="file"
              id="file"
              accept=".pdf,.docx,.doc,.txt"
              onChange={(e) => {
                const selectedFile = e.target.files?.[0];
                setFile(selectedFile || null);
                if (selectedFile && !title) {
                  setTitle(selectedFile.name.replace(/\.[^/.]+$/, ''));
                }
              }}
              required
            />
            <small>Supported formats: PDF, DOCX, DOC, TXT (Max 10MB)</small>
          </div>

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

          <div className="modal-footer">
            <button type="button" className="btn btn-outline" onClick={onClose} disabled={uploadMutation.isPending}>
              Cancel
            </button>
            <button type="submit" className="btn btn-primary" disabled={uploadMutation.isPending}>
              {uploadMutation.isPending ? 'Uploading...' : 'Upload'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};
