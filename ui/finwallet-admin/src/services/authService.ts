import { requestJson } from './apiClient';
import type { AuthResult, TotpSetup } from '../types/auth';
import { storage } from './storage';

export const authService = {
  async login(email: string, password: string, totpCode?: string): Promise<AuthResult> {
    return requestJson<AuthResult>('/api/auth/login', {
      method: 'POST',
      body: {
        email,
        password,
        totpCode: totpCode || null
      },
      auth: false
    });
  },
  async refresh(): Promise<AuthResult> {
    const refreshToken = storage.getRefreshToken();
    if (!refreshToken) {
      throw new Error('Refresh token missing');
    }

    return requestJson<AuthResult>('/api/auth/refresh', {
      method: 'POST',
      body: { refreshToken },
      auth: false
    });
  },
  async logout(): Promise<boolean> {
    const refreshToken = storage.getRefreshToken();
    return requestJson<boolean>('/api/auth/logout', {
      method: 'POST',
      body: { refreshToken: refreshToken ?? '' },
      auth: false
    });
  },
  async setupTotp(): Promise<TotpSetup> {
    return requestJson<TotpSetup>('/api/auth/totp/setup', {
      method: 'POST',
      body: {}
    });
  },
  async confirmTotp(code: string): Promise<boolean> {
    return requestJson<boolean>('/api/auth/totp/confirm', {
      method: 'POST',
      body: { code }
    });
  }
};
