import { Injectable, PLATFORM_ID, inject } from '@angular/core';
import { Router } from '@angular/router';
import { BehaviorSubject, combineLatest, filter, map, Observable, take, tap } from 'rxjs';
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

  // En AuthService, modifica:
  public isLoggedIn$ = this.user$.pipe(map((user) => !!user));

  constructor() {
    this.autoLogin();
  }

  private loading$ = new BehaviorSubject<boolean>(false);
  public isLoading$ = this.loading$.asObservable();

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

  logout(): void {
    localStorage.removeItem('token');
    localStorage.removeItem('expires');
    this.user$.next(null);
    this.router.navigate(['/main']);
  }

  private handleAuth(res: TokenResponse): void {
    // Guardamos JWT
    localStorage.setItem('token', res.token);
    localStorage.setItem('expires', res.expires);

    // Decodificamos payload del JWT para sacar info del usuario
    const payload = JSON.parse(atob(res.token.split('.')[1])) as any;
    const user: User = {
      id: payload.sub || payload.nameidentifier, // por seguridad soporta sub o nameidentifier
      email: payload.email,
      fullName: payload.name,
    };
    this.user$.next(user);
  }

  autoLogin(): void {
    if (!this.isBrowser) {
      // En SSR inicializamos sin usuario; la validación real la hace el cliente.
      this.user$.next(null);
      this.initialized$.next(true);
      return;
    }

    // Solo en el cliente (browser)
    const token = localStorage.getItem('token');
    const exp = localStorage.getItem('expires');

    if (!token || !exp || new Date() >= new Date(exp)) {
      this.user$.next(null);
    } else {
      this.handleAuth({ token, expires: exp });
    }

    this.initialized$.next(true);
  }

  getToken(): string | null {
    return this.isBrowser ? localStorage.getItem('token') : null;
  }

  // Actualiza en memoria el usuario actual (p. ej. tras editar el perfil)
  patchUser(patch: Partial<User>): void {
    const current = this.user$.value;
    if (current) this.user$.next({ ...current, ...patch });
  }
}
