import { authEvents } from './authEvents';
import { storage } from './storage';
import type { AuthResult } from '../types/auth';

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000';

let isRefreshing = false;
let refreshPromise: Promise<string | null> | null = null;

interface RequestOptions {
  method?: string;
  body?: unknown;
  headers?: Record<string, string>;
  auth?: boolean;
}

async function refreshAccessToken(): Promise<string | null> {
  const refreshToken = storage.getRefreshToken();
  if (!refreshToken) {
    return null;
  }

  const response = await fetch(`${API_BASE_URL}/api/auth/refresh`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({ refreshToken })
  });

  if (!response.ok) {
    return null;
  }

  const data = (await response.json()) as AuthResult;
  if (data.accessToken && data.refreshToken) {
    storage.setSession(data.accessToken, data.refreshToken, data.email);
    return data.accessToken;
  }

  return null;
}

async function parseError(response: Response) {
  let message = 'Unexpected error';
  try {
    const contentType = response.headers.get('content-type');
    if (contentType && contentType.includes('application/json')) {
      const data = await response.json();
      message = data?.detail || data?.title || message;
    }
  } catch {
    message = 'Unexpected error';
  }
  const error = new Error(message);
  (error as Error & { status?: number }).status = response.status;
  throw error;
}

export async function requestJson<T>(path: string, options: RequestOptions = {}): Promise<T> {
  const method = options.method ?? 'GET';
  const requiresAuth = options.auth ?? true;
  const isAuthRequest = path.includes('/api/auth/login') || path.includes('/api/auth/refresh');

  const headers: Record<string, string> = {
    Accept: 'application/json',
    ...(options.headers ?? {})
  };

  if (options.body !== undefined) {
    headers['Content-Type'] = 'application/json';
  }

  const accessToken = storage.getAccessToken();
  if (requiresAuth && accessToken && !isAuthRequest) {
    headers.Authorization = `Bearer ${accessToken}`;
  }

  const response = await fetch(`${API_BASE_URL}${path}`, {
    method,
    headers,
    body: options.body !== undefined ? JSON.stringify(options.body) : undefined
  });

  if (response.status === 401 && requiresAuth && !isAuthRequest) {
    if (!isRefreshing) {
      isRefreshing = true;
      refreshPromise = refreshAccessToken().finally(() => {
        isRefreshing = false;
      });
    }

    const newToken = await refreshPromise;
    refreshPromise = null;
    if (!newToken) {
      storage.clear();
      authEvents.emitSessionCleared();
      await parseError(response);
    }

    const retryHeaders = {
      ...headers,
      Authorization: `Bearer ${newToken}`
    };

    const retryResponse = await fetch(`${API_BASE_URL}${path}`, {
      method,
      headers: retryHeaders,
      body: options.body !== undefined ? JSON.stringify(options.body) : undefined
    });

    if (!retryResponse.ok) {
      await parseError(retryResponse);
    }

    if (retryResponse.status === 204) {
      return undefined as T;
    }

    return (await retryResponse.json()) as T;
  }

  if (!response.ok) {
    await parseError(response);
  }

  if (response.status === 204) {
    return undefined as T;
  }

  return (await response.json()) as T;
}

export async function requestBlob(path: string, options: RequestOptions = {}): Promise<Blob> {
  const method = options.method ?? 'GET';
  const requiresAuth = options.auth ?? true;

  const headers: Record<string, string> = {
    ...(options.headers ?? {})
  };

  const accessToken = storage.getAccessToken();
  if (requiresAuth && accessToken) {
    headers.Authorization = `Bearer ${accessToken}`;
  }

  const response = await fetch(`${API_BASE_URL}${path}`, {
    method,
    headers,
    body: options.body !== undefined ? JSON.stringify(options.body) : undefined
  });

  if (!response.ok) {
    await parseError(response);
  }

  return await response.blob();
}
