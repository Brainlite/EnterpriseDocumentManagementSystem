import { createContext, useContext, useState, useEffect } from 'react';
import type { ReactNode } from 'react';
import type { UserPayload } from '../types/auth';

interface AuthContextType {
  user: UserPayload | null;
  token: string | null;
  login: (token: string, user: UserPayload) => void;
  logout: () => void;
  isAuthenticated: boolean;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const AuthProvider = ({ children }: { children: ReactNode }) => {
  const [user, setUser] = useState<UserPayload | null>(() => {
    const storedUser = localStorage.getItem('user');
    return storedUser ? JSON.parse(storedUser) : null;
  });
  const [token, setToken] = useState<string | null>(() => {
    return localStorage.getItem('authToken');
  });

  useEffect(() => {
    const handleStorageChange = () => {
      const currentToken = localStorage.getItem('authToken');
      const currentUser = localStorage.getItem('user');
      
      if (!currentToken) {
        setToken(null);
        setUser(null);
      } else if (currentToken !== token || currentUser !== JSON.stringify(user)) {
        setToken(currentToken);
        setUser(currentUser ? JSON.parse(currentUser) : null);
      }
    };

    // Listen for storage changes from other tabs
    window.addEventListener('storage', handleStorageChange);

    return () => {
      window.removeEventListener('storage', handleStorageChange);
    };
  }, [token, user]);

  const login = (newToken: string, newUser: UserPayload) => {
    localStorage.setItem('authToken', newToken);
    localStorage.setItem('user', JSON.stringify(newUser));
    setToken(newToken);
    setUser(newUser);
  };

  const logout = () => {
    localStorage.removeItem('authToken');
    localStorage.removeItem('user');
    setToken(null);
    setUser(null);
  };

  return (
    <AuthContext.Provider
      value={{
        user,
        token,
        login,
        logout,
        isAuthenticated: !!token && !!user,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};
