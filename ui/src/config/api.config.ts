export const API_CONFIG = {
  BASE_URL: import.meta.env.VITE_API_URL || 'https://localhost:7052',
  ENDPOINTS: {
    AUTH: '/api/auth',
    DOCUMENTS: '/api/documents',
    TAGS: '/api/tags',
  },
} as const;

export const getApiUrl = (endpoint: string) => {
  return `${API_CONFIG.BASE_URL}${endpoint}`;
};
