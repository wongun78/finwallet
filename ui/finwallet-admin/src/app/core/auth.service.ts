import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap, throwError } from 'rxjs';
import { Router } from '@angular/router';
import { ApiService } from './api.service';
import { StorageService } from './storage.service';
import { AuthResult, TotpSetup } from '../models/auth.model';

@Injectable({ providedIn: 'root' })
export class AuthService {
  constructor(
    private http: HttpClient,
    private api: ApiService,
    private storage: StorageService,
    private router: Router
  ) {}

  login(email: string, password: string, totpCode?: string): Observable<AuthResult> {
    return this.http
      .post<AuthResult>(`${this.api.baseUrl}/api/auth/login`, {
        email,
        password,
        totpCode: totpCode || null
      })
      .pipe(
        tap((result) => {
          if (result.accessToken && result.refreshToken) {
            this.storage.setSession(result.accessToken, result.refreshToken, result.email);
          }
        })
      );
  }

  refresh(): Observable<AuthResult> {
    const refreshToken = this.storage.getRefreshToken();
    if (!refreshToken) {
      return throwError(() => new Error('Refresh token missing'));
    }
    return this.http
      .post<AuthResult>(`${this.api.baseUrl}/api/auth/refresh`, {
        refreshToken
      })
      .pipe(
        tap((result) => {
          if (result.accessToken && result.refreshToken) {
            this.storage.setSession(result.accessToken, result.refreshToken, result.email);
          }
        })
      );
  }

  logout(): Observable<boolean> {
    const refreshToken = this.storage.getRefreshToken();
    return this.http
      .post<boolean>(`${this.api.baseUrl}/api/auth/logout`, {
        refreshToken: refreshToken ?? ''
      })
      .pipe(
        tap(() => this.storage.clear())
      );
  }

  setupTotp(): Observable<TotpSetup> {
    return this.http.post<TotpSetup>(`${this.api.baseUrl}/api/auth/totp/setup`, {});
  }

  confirmTotp(code: string): Observable<boolean> {
    return this.http.post<boolean>(`${this.api.baseUrl}/api/auth/totp/confirm`, { code });
  }

  redirectToLogin(): void {
    this.storage.clear();
    this.router.navigateByUrl('/login');
  }

  isLoggedIn(): boolean {
    return !!this.storage.getAccessToken();
  }
}
