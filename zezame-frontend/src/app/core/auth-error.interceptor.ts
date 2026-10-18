import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { catchError, switchMap, throwError } from 'rxjs';
import { AuthService } from './auth.service';

/**
 * Ante un 401 intenta renovar el access token (una sola vez, compartido) y
 * reintenta la petición original. Si la renovación falla, cierra sesión.
 * No actúa sobre los propios endpoints /auth/ ni en SSR.
 */
export const authErrorInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const isBrowser = isPlatformBrowser(inject(PLATFORM_ID));

  return next(req).pipe(
    catchError((err: HttpErrorResponse) => {
      const isAuthCall = req.url.includes('/auth/');

      if (err.status === 401 && isBrowser && !isAuthCall) {
        return auth.refreshAccessToken().pipe(
          switchMap((token) => {
            if (!token) {
              auth.forceLogout();
              return throwError(() => err);
            }
            const retried = req.clone({ setHeaders: { Authorization: `Bearer ${token}` } });
            return next(retried);
          }),
        );
      }

      return throwError(() => err);
    }),
  );
};
