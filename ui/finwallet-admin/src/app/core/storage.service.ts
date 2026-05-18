import { Injectable } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class StorageService {
  private readonly accessTokenKey = 'finwallet_access_token';
  private readonly refreshTokenKey = 'finwallet_refresh_token';
  private readonly emailKey = 'finwallet_email';

  setSession(accessToken: string, refreshToken: string, email: string): void {
    localStorage.setItem(this.accessTokenKey, accessToken);
    localStorage.setItem(this.refreshTokenKey, refreshToken);
    localStorage.setItem(this.emailKey, email);
  }

  getAccessToken(): string | null {
    return localStorage.getItem(this.accessTokenKey);
  }

  getRefreshToken(): string | null {
    return localStorage.getItem(this.refreshTokenKey);
  }

  getEmail(): string | null {
    return localStorage.getItem(this.emailKey);
  }

  clear(): void {
    localStorage.removeItem(this.accessTokenKey);
    localStorage.removeItem(this.refreshTokenKey);
    localStorage.removeItem(this.emailKey);
  }
}
