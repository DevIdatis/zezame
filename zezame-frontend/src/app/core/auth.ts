import { Injectable, inject } from '@angular/core';
import { Router } from '@angular/router';
import { BehaviorSubject, map, Observable, tap } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { LoginRequest, RegisterRequest, TokenResponse, User } from './models';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly api = environment.apiUrl;

  private user$ = new BehaviorSubject<User | null>(null);
  public currentUser$ = this.user$.asObservable();
  public isLoggedIn$ = this.user$.pipe(map((u) => !!u));

  login(body: LoginRequest): Observable<TokenResponse> {
    return this.http
      .post<TokenResponse>(`${this.api}/auth/login`, body)
      .pipe(tap((res) => this.handleAuth(res)));
  }

  register(body: RegisterRequest): Observable<TokenResponse> {
    return this.http
      .post<TokenResponse>(`${this.api}/auth/register`, body)
      .pipe(tap((res) => this.handleAuth(res)));
  }

  logout(): void {
    localStorage.removeItem('token');
    localStorage.removeItem('expires');
    this.user$.next(null);
    //this.router.navigate(['/login']);
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
    if (typeof window === 'undefined') return; // protección SSR
    const token = localStorage.getItem('token');
    const exp = localStorage.getItem('expires');

    if (!token || !exp || new Date() >= new Date(exp)) {
      this.logout();
      return;
    }

    this.handleAuth({ token, expires: exp });
  }

  getToken(): string | null {
    return localStorage.getItem('token');
  }
}
