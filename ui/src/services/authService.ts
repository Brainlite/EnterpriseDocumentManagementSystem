import { apiClient } from './api';
import type { LoginRequest, LoginResponse, UserPayload } from '../types/auth';

export const authService = {
  login: async (credentials: LoginRequest): Promise<LoginResponse> => {
    const response = await apiClient.post<LoginResponse>('/auth/login', credentials);
    return response.data;
  },

  logout: async (): Promise<void> => {
    await apiClient.post('/auth/logout');
  },

  getCurrentUser: async (): Promise<UserPayload> => {
    const response = await apiClient.get<UserPayload>('/auth/me');
    return response.data;
  },
};
