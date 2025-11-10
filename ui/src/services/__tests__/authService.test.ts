import { describe, it, expect, vi, beforeEach } from 'vitest';
import { authService } from '../authService';
import { apiClient } from '../api';
import type { LoginRequest, LoginResponse, UserPayload } from '../../types/auth';

vi.mock('../api');

describe('authService', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe('login', () => {
    it('should successfully login with valid credentials', async () => {
      // Arrange
      const credentials: LoginRequest = {
        email: 'admin@example.com',
        password: 'admin123',
      };

      const mockResponse: LoginResponse = {
        token: 'mock-jwt-token',
        user: {
          id: '1',
          email: 'admin@example.com',
          name: 'Admin User',
          role: 'Admin',
        },
      };

      vi.mocked(apiClient.post).mockResolvedValue({ data: mockResponse });

      // Act
      const result = await authService.login(credentials);

      // Assert
      expect(apiClient.post).toHaveBeenCalledWith('/auth/login', credentials);
      expect(result).toEqual(mockResponse);
      expect(result.token).toBe('mock-jwt-token');
      expect(result.user.email).toBe('admin@example.com');
    });

    it('should throw error on failed login', async () => {
      // Arrange
      const credentials: LoginRequest = {
        email: 'invalid@example.com',
        password: 'wrongpassword',
      };

      vi.mocked(apiClient.post).mockRejectedValue(new Error('Unauthorized'));

      // Act & Assert
      await expect(authService.login(credentials)).rejects.toThrow('Unauthorized');
    });
  });

  describe('logout', () => {
    it('should call logout endpoint', async () => {
      // Arrange
      vi.mocked(apiClient.post).mockResolvedValue({ data: {} });

      // Act
      await authService.logout();

      // Assert
      expect(apiClient.post).toHaveBeenCalledWith('/auth/logout');
    });
  });

  describe('getCurrentUser', () => {
    it('should return current user data', async () => {
      // Arrange
      const mockUser: UserPayload = {
        id: '1',
        email: 'admin@example.com',
        name: 'Admin User',
        role: 'Admin',
      };

      vi.mocked(apiClient.get).mockResolvedValue({ data: mockUser });

      // Act
      const result = await authService.getCurrentUser();

      // Assert
      expect(apiClient.get).toHaveBeenCalledWith('/auth/me');
      expect(result).toEqual(mockUser);
    });

    it('should throw error when user is not authenticated', async () => {
      // Arrange
      vi.mocked(apiClient.get).mockRejectedValue(new Error('Unauthorized'));

      // Act & Assert
      await expect(authService.getCurrentUser()).rejects.toThrow('Unauthorized');
    });
  });
});
