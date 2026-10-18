import { Injectable, PLATFORM_ID, inject } from '@angular/core';
import { Router } from '@angular/router';
import { BehaviorSubject, catchError, finalize, map, Observable, of, shareReplay, tap } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { LoginRequest, RegisterRequest, TokenResponse, User } from './models';
import { isPlatformBrowser } from '@angular/common';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly api = environment.apiUrl;
  private platformId = inject(PLATFORM_ID);
  private isBrowser = isPlatformBrowser(this.platformId);

  private user$ = new BehaviorSubject<User | null>(null);
  public currentUser$ = this.user$.asObservable();
  private initialized$ = new BehaviorSubject<boolean>(false);

  public isLoggedIn$ = this.user$.pipe(map((user) => !!user));

  private loading$ = new BehaviorSubject<boolean>(false);
  public isLoading$ = this.loading$.asObservable();

  // Llamada de refresco en curso, compartida entre peticiones concurrentes.
  private refreshInFlight: Observable<string | null> | null = null;

  constructor() {
    this.autoLogin();
  }

  login(body: LoginRequest): Observable<TokenResponse> {
    this.loading$.next(true);
    return this.http.post<TokenResponse>(`${this.api}/auth/login`, body).pipe(
      tap((res) => this.handleAuth(res)),
      tap({ finalize: () => this.loading$.next(false) }),
    );
  }

  register(body: RegisterRequest): Observable<TokenResponse> {
    this.loading$.next(true);
    return this.http.post<TokenResponse>(`${this.api}/auth/register`, body).pipe(
      tap((res) => this.handleAuth(res)),
      tap({ finalize: () => this.loading$.next(false) }),
    );
  }

  /** Cierre iniciado por el usuario: revoca el refresh token en el backend. */
  logout(): void {
    const rt = this.getRefreshToken();
    if (this.isBrowser && rt) {
      this.http.post(`${this.api}/auth/logout`, { refreshToken: rt }).subscribe({
        next: () => {},
        error: () => {},
      });
    }
    this.forceLogout();
  }

  /** Cierre local (sesión caducada o refresco fallido): limpia y redirige sin llamar al backend. */
  forceLogout(): void {
    this.clearSession();
    this.router.navigate(['/main']);
  }

  private clearSession(): void {
    if (this.isBrowser) {
      localStorage.removeItem('token');
      localStorage.removeItem('expires');
      localStorage.removeItem('refreshToken');
    }
    this.user$.next(null);
  }

  /**
   * Renueva el access token con el refresh token. Comparte una única llamada en
   * curso entre peticiones concurrentes para no refrescar en paralelo.
   * Devuelve el nuevo access token, o null si no se pudo renovar.
   */
  refreshAccessToken(): Observable<string | null> {
    if (this.refreshInFlight) return this.refreshInFlight;

    const rt = this.getRefreshToken();
    if (!this.isBrowser || !rt) return of(null);

    this.refreshInFlight = this.http
      .post<TokenResponse>(`${this.api}/auth/refresh`, { refreshToken: rt })
      .pipe(
        tap((res) => this.handleAuth(res)),
        map((res) => res.token as string | null),
        catchError(() => of(null)),
        finalize(() => (this.refreshInFlight = null)),
        shareReplay(1),
      );

    return this.refreshInFlight;
  }

  private handleAuth(res: TokenResponse): void {
    if (this.isBrowser) {
      localStorage.setItem('token', res.token);
      localStorage.setItem('expires', res.expires);
      if (res.refreshToken) localStorage.setItem('refreshToken', res.refreshToken);
    }
    this.setUserFromToken(res.token);
  }

  private setUserFromToken(token: string): void {
    try {
      const payload = JSON.parse(atob(token.split('.')[1])) as any;
      this.user$.next({
        id: payload.sub || payload.nameidentifier,
        email: payload.email,
        fullName: payload.name,
      });
    } catch {
      this.user$.next(null);
    }
  }

  autoLogin(): void {
    if (!this.isBrowser) {
      // En SSR inicializamos sin usuario; la validación real la hace el cliente.
      this.user$.next(null);
      this.initialized$.next(true);
      return;
    }

    const token = localStorage.getItem('token');
    const exp = localStorage.getItem('expires');

    if (!token) {
      this.user$.next(null);
    } else if (!exp || new Date() >= new Date(exp)) {
      // Access token caducado: mantenemos la sesión en memoria; la primera
      // petición protegida disparará el refresco (interceptor 401).
      this.setUserFromToken(token);
    } else {
      this.setUserFromToken(token);
    }

    this.initialized$.next(true);
  }

  getToken(): string | null {
    return this.isBrowser ? localStorage.getItem('token') : null;
  }

  getRefreshToken(): string | null {
    return this.isBrowser ? localStorage.getItem('refreshToken') : null;
  }

  verifyEmail(token: string): Observable<unknown> {
    return this.http.post(`${this.api}/auth/verify-email`, { token });
  }

  forgotPassword(email: string): Observable<unknown> {
    return this.http.post(`${this.api}/auth/forgot-password`, { email });
  }

  resetPassword(token: string, newPassword: string): Observable<unknown> {
    return this.http.post(`${this.api}/auth/reset-password`, { token, newPassword });
  }

  // Actualiza en memoria el usuario actual (p. ej. tras editar el perfil)
  patchUser(patch: Partial<User>): void {
    const current = this.user$.value;
    if (current) this.user$.next({ ...current, ...patch });
  }
}
