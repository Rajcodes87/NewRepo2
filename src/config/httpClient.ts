import axios, { AxiosInstance, AxiosError } from 'axios';
import { ResponseDataDto } from '../types/common';

// Create axios instance with default config
const httpClient: AxiosInstance = axios.create({
  baseURL: 'https://localhost:44365/api',
  timeout: 30000,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Request interceptor - add auth token if available
httpClient.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('access_token');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Response interceptor - handle errors globally
httpClient.interceptors.response.use(
  (response) => {
    return response;
  },
  (error: AxiosError<any>) => {
    if (error.response?.status === 401) {
      // Handle unauthorized - redirect to login
      localStorage.removeItem('access_token');
      window.location.href = '/login';
    }

    // Extract error message from response
    let errorMessage = 'An error occurred';
    if (error.response?.data) {
      const data = error.response.data;
      // ABP error response format
      if (data.error?.message) {
        errorMessage = data.error.message;
      } else if (data.message) {
        errorMessage = data.message;
      } else if (typeof data === 'string') {
        errorMessage = data;
      }
      // Add validation errors if present
      if (data.error?.validationErrors && Array.isArray(data.error.validationErrors)) {
        const validationMessages = data.error.validationErrors
          .map((v: any) => v.message)
          .join(', ');
        errorMessage += ': ' + validationMessages;
      }
    } else if (error.message) {
      errorMessage = error.message;
    }

    // Create a new error with the extracted message
    const enhancedError: any = new Error(errorMessage);
    enhancedError.response = error.response;
    enhancedError.status = error.response?.status;

    return Promise.reject(enhancedError);
  }
);

// Helper function to extract data from ResponseDataDto
export const extractData = <T>(response: ResponseDataDto<T>): T => {
  if (!response.success) {
    const errorMessage = response.message || 'An error occurred';
    throw new Error(errorMessage);
  }
  return response.data as T;
};

export default httpClient;
