import { inject } from '@angular/core';
import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { BehaviorSubject, catchError, filter, switchMap, take, throwError, finalize } from 'rxjs';
import { StorageService } from './storage.service';
import { AuthService } from './auth.service';

let isRefreshing = false;
const refreshTokenSubject = new BehaviorSubject<string | null>(null);

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const storage = inject(StorageService);
  const auth = inject(AuthService);
  const accessToken = storage.getAccessToken();

  const isAuthRequest = req.url.includes('/api/auth/login') || req.url.includes('/api/auth/refresh');
  const authReq = accessToken && !isAuthRequest
    ? req.clone({ setHeaders: { Authorization: `Bearer ${accessToken}` } })
    : req;

  return next(authReq).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status !== 401 || isAuthRequest) {
        return throwError(() => error);
      }

      if (!isRefreshing) {
        isRefreshing = true;
        refreshTokenSubject.next(null);

        return auth.refresh().pipe(
          switchMap(() => {
            const newToken = storage.getAccessToken();
            if (!newToken) {
              auth.redirectToLogin();
              return throwError(() => error);
            }

            refreshTokenSubject.next(newToken);
            const retryReq = req.clone({ setHeaders: { Authorization: `Bearer ${newToken}` } });
            return next(retryReq);
          }),
          catchError((refreshError) => {
            auth.redirectToLogin();
            return throwError(() => refreshError);
          }),
          finalize(() => {
            isRefreshing = false;
          })
        );
      }

      return refreshTokenSubject.pipe(
        filter((token): token is string => !!token),
        take(1),
        switchMap((token) =>
          next(req.clone({ setHeaders: { Authorization: `Bearer ${token}` } }))
        )
      );
    })
  );
};
