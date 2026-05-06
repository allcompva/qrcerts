import axios, { AxiosInstance, AxiosResponse } from 'axios';
import { API_BASE_URL, API_ENDPOINTS } from '../config/api';
import { AuthResponse, User } from '../types';

class ApiService {
  private api: AxiosInstance;

  constructor() {
    this.api = axios.create({
      baseURL: API_BASE_URL,
      headers: {
        'Content-Type': 'application/json',
      },
    });

    // Add auth token to requests
    this.api.interceptors.request.use((config) => {
      const token = localStorage.getItem('token');
      if (token) {
        config.headers.Authorization = `Bearer ${token}`;
      }
      return config;
    });

    // Handle auth errors
    this.api.interceptors.response.use(
      (response) => response,
      (error) => {
        // Only redirect on 401 if it's not a login attempt
        const isLoginRequest = error.config?.url?.includes('/auth/login');

        if (error.response?.status === 401 && !isLoginRequest) {
          localStorage.removeItem('token');
          localStorage.removeItem('user');
          window.location.href = '/#/admin/login';
        }
        return Promise.reject(error);
      }
    );
  }

  // Auth methods
  async adminLogin(username: string, password: string): Promise<AuthResponse> {
    const response = await this.api.post(API_ENDPOINTS.ADMIN_LOGIN, {
      username,
      password,
    });
    return response.data;
  }

  async otecLogin(username: string, password: string): Promise<AuthResponse> {
    const response = await this.api.post(API_ENDPOINTS.OTEC_LOGIN, {
      username,
      password,
    });
    return response.data;
  }

  // Generic CRUD methods
  async get<T>(endpoint: string): Promise<T> {
    const response: AxiosResponse<T> = await this.api.get(endpoint);
    return response.data;
  }

  async post<T>(endpoint: string, data: any): Promise<T> {
    const response: AxiosResponse<T> = await this.api.post(endpoint, data);
    return response.data;
  }

  async put<T>(endpoint: string, data: any): Promise<T> {
    const response: AxiosResponse<T> = await this.api.put(endpoint, data);
    return response.data;
  }

  async delete<T>(endpoint: string): Promise<T> {
    const response: AxiosResponse<T> = await this.api.delete(endpoint);
    return response.data;
  }

  // File upload
  async uploadFile(endpoint: string, file: File): Promise<any> {
    const formData = new FormData();
    formData.append('file', file);

    const response = await this.api.post(endpoint, formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
    return response.data;
  }
}

export const apiService = new ApiService();
export default apiService;