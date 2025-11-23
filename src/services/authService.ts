import axios from 'axios';
import httpClient from '../config/httpClient';
import { ResponseDataDto } from '../types/common';

const AUTH_SERVER_URL = 'https://localhost:44363';

export interface LoginRequest {
  username: string;
  password: string;
}

export interface TokenResponse {
  access_token: string;
  token_type: string;
  expires_in: number;
  refresh_token?: string;
  scope: string;
}

export interface RegisterRescuerRequest {
  userName: string;
  email: string;
  password: string;
  name?: string;
  surname?: string;
  phoneNumber?: string;
}

export interface RegisterRescuerResponse {
  userId: string;
  userName: string;
  email: string;
}

export interface UserInfo {
  sub: string;
  name?: string;
  email?: string;
  role: string | string[];
}

class AuthService {
  // Login with username and password
  async login(username: string, password: string): Promise<TokenResponse> {
    const formData = new URLSearchParams();
    formData.append('grant_type', 'password');
    formData.append('client_id', 'Pawchums_App');
    formData.append('username', username);
    formData.append('password', password);
    formData.append('scope', 'offline_access Pawchums');

    const response = await axios.post<TokenResponse>(
      `${AUTH_SERVER_URL}/connect/token`,
      formData,
      {
        headers: {
          'Content-Type': 'application/x-www-form-urlencoded',
        },
      }
    );

    const tokenData = response.data;

    // Store tokens
    localStorage.setItem('access_token', tokenData.access_token);
    if (tokenData.refresh_token) {
      localStorage.setItem('refresh_token', tokenData.refresh_token);
    }

    // Decode and store user info
    const userInfo = this.decodeToken(tokenData.access_token);
    localStorage.setItem('user_info', JSON.stringify(userInfo));

    return tokenData;
  }

  // Register as Rescuer
  async registerRescuer(data: RegisterRescuerRequest): Promise<ResponseDataDto<RegisterRescuerResponse>> {
    const response = await httpClient.post<ResponseDataDto<RegisterRescuerResponse>>(
      '/app/rescuer-registration/register',
      data
    );
    return response.data;
  }

  // Verify email with code
  async verifyEmail(email: string, code: string): Promise<ResponseDataDto<object>> {
    const response = await httpClient.post<ResponseDataDto<object>>(
      '/app/rescuer-registration/verify-email',
      { email, code }
    );
    return response.data;
  }

  // Request password reset code
  async forgotPassword(email: string): Promise<ResponseDataDto<object>> {
    const response = await httpClient.post<ResponseDataDto<object>>(
      '/app/rescuer-registration/forgot-password',
      { email }
    );
    return response.data;
  }

  // Reset password with verification code
  async resetPassword(email: string, code: string, newPassword: string): Promise<ResponseDataDto<object>> {
    const response = await httpClient.post<ResponseDataDto<object>>(
      '/app/rescuer-registration/reset-password',
      { email, code, newPassword }
    );
    return response.data;
  }

  // Logout
  logout(): void {
    localStorage.removeItem('access_token');
    localStorage.removeItem('refresh_token');
    localStorage.removeItem('user_info');
  }

  // Check if user is authenticated
  isAuthenticated(): boolean {
    const token = localStorage.getItem('access_token');
    if (!token) return false;

    try {
      const userInfo = this.decodeToken(token);
      // Check if token is expired
      const exp = userInfo.exp;
      if (exp && Date.now() >= exp * 1000) {
        this.logout();
        return false;
      }
      return true;
    } catch {
      return false;
    }
  }

  // Get current user info
  getCurrentUser(): UserInfo | null {
    const userInfoStr = localStorage.getItem('user_info');
    if (!userInfoStr) return null;

    try {
      return JSON.parse(userInfoStr);
    } catch {
      return null;
    }
  }

  // Get user role
  getUserRole(): string | string[] | null {
    const userInfo = this.getCurrentUser();
    return userInfo?.role || null;
  }

  // Check if user has a specific role
  hasRole(role: string): boolean {
    const userRole = this.getUserRole();
    if (!userRole) return false;

    if (Array.isArray(userRole)) {
      return userRole.includes(role);
    }
    return userRole === role;
  }

  // Decode JWT token
  private decodeToken(token: string): any {
    try {
      const base64Url = token.split('.')[1];
      const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
      const jsonPayload = decodeURIComponent(
        atob(base64)
          .split('')
          .map((c) => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
          .join('')
      );
      return JSON.parse(jsonPayload);
    } catch (error) {
      console.error('Error decoding token:', error);
      return {};
    }
  }
}

export default new AuthService();
