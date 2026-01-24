import { inject, PLATFORM_ID } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from './auth.service';
import { map, first, take, tap, filter } from 'rxjs';
import { isPlatformBrowser } from '@angular/common';

// En auth.guard.ts
export const authGuard: CanActivateFn = (route, state) => {
  const auth = inject(AuthService);
  const router = inject(Router);
  const platformId = inject(PLATFORM_ID);
  const isBrowser = isPlatformBrowser(platformId);

  console.log('Guard ejecutándose. ¿Es cliente?:', isBrowser);

  // Si no estamos en el navegador (SSR), permitimos la navegación
  // y dejamos que el cliente haga la validación real
  if (!isBrowser) {
    console.log('SSR: Permitiendo acceso temporal');
    return true; // O podrías retornar router.createUrlTree(['/main']) si quieres bloquear en SSR
  }

  return auth.isLoggedIn$.pipe(
    filter((isLoggedIn) => isLoggedIn !== undefined), // Espera valor definido
    first(),
    map((isLoggedIn) => {
      console.log('Guard evaluando (cliente):', isLoggedIn);
      return isLoggedIn ? true : router.createUrlTree(['/main']);
    }),
  );
};
