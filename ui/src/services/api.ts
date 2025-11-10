import axios from 'axios';
import { getApiUrl } from '../config/api.config';

export const apiClient = axios.create({
  baseURL: getApiUrl('/api'),
  headers: {
    'Content-Type': 'application/json',
  },
  withCredentials: true,
});

// Request interceptor to add auth token
apiClient.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('authToken');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Response interceptor to handle errors
apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401 && !error.config?.url?.includes('/auth/login')) {
      const hasToken = localStorage.getItem('authToken');
      if (hasToken) {
        localStorage.removeItem('authToken');
        localStorage.removeItem('user');
      }
    }
    return Promise.reject(error);
  }
);
