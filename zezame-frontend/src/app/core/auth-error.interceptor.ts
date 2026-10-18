import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { catchError, throwError } from 'rxjs';
import { AuthService } from './auth.service';

/**
 * Cierra la sesión y redirige cuando la API responde 401 (token ausente,
 * inválido o caducado). Solo actúa en el navegador: en SSR no hay sesión
 * que cerrar y localStorage no está disponible.
 */
export const authErrorInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const isBrowser = isPlatformBrowser(inject(PLATFORM_ID));

  return next(req).pipe(
    catchError((err: HttpErrorResponse) => {
      if (err.status === 401 && isBrowser) {
        auth.logout();
      }
      return throwError(() => err);
    }),
  );
};
