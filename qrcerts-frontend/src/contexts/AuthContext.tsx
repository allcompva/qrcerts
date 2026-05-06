import React, { createContext, useContext, useEffect, useState } from 'react';
import { User, AuthResponse } from '../types';
import { apiService } from '../services/api';

interface AuthContextType {
  user: User | null;
  isLoading: boolean;
  adminLogin: (username: string, password: string) => Promise<void>;
  otecLogin: (username: string, password: string) => Promise<void>;
  logout: () => void;
  isAuthenticated: boolean;
  isAdmin: boolean;
  isOtec: boolean;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [user, setUser] = useState<User | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    // Check if user is logged in on app start
    const token = localStorage.getItem('token');
    const userStr = localStorage.getItem('user');

    if (token && userStr) {
      try {
        const userData = JSON.parse(userStr);
        setUser(userData);
      } catch (error) {
        localStorage.removeItem('token');
        localStorage.removeItem('user');
      }
    }
    setIsLoading(false);
  }, []);

  const adminLogin = async (username: string, password: string) => {
    try {
      const response: AuthResponse = await apiService.adminLogin(username, password);
      localStorage.setItem('token', response.token);
      localStorage.setItem('user', JSON.stringify(response.user));
      setUser(response.user);
    } catch (error) {
      throw error;
    }
  };

  const otecLogin = async (username: string, password: string) => {
    try {
      const response: AuthResponse = await apiService.otecLogin(username, password);
      localStorage.setItem('token', response.token);
      localStorage.setItem('user', JSON.stringify(response.user));
      setUser(response.user);
    } catch (error) {
      throw error;
    }
  };

  const logout = () => {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    setUser(null);
  };

  const value: AuthContextType = {
    user,
    isLoading,
    adminLogin,
    otecLogin,
    logout,
    isAuthenticated: !!user,
    isAdmin: user?.role === 'admin',
    isOtec: user?.role === 'otec',
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
}