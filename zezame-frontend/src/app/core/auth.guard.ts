import { inject, PLATFORM_ID } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from './services/auth.service';
import { map, first, take, tap, filter } from 'rxjs';
import { isPlatformBrowser } from '@angular/common';

// En auth.guard.ts
export const authGuard: CanActivateFn = (route, state) => {
  const auth = inject(AuthService);
  const router = inject(Router);
  const platformId = inject(PLATFORM_ID);
  const isBrowser = isPlatformBrowser(platformId);

  // En SSR permitimos la navegación; la validación real la hace el cliente.
  if (!isBrowser) {
    return true; 
  }

  return auth.isLoggedIn$.pipe(
    filter((isLoggedIn) => isLoggedIn !== undefined), 
    first(),
    map((isLoggedIn) => (isLoggedIn ? true : router.createUrlTree(['/main']))),
  );
};
