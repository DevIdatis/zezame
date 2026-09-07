import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { ChangePasswordRequest, Profile, UpdateProfileRequest } from './models';

@Injectable({ providedIn: 'root' })
export class UserService {
  private http = inject(HttpClient);
  private api = environment.apiUrl;

  getMe(): Observable<Profile> {
    return this.http.get<Profile>(`${this.api}/users/me`);
  }

  updateMe(body: UpdateProfileRequest): Observable<Profile> {
    return this.http.put<Profile>(`${this.api}/users/me`, body);
  }

  changePassword(body: ChangePasswordRequest): Observable<void> {
    return this.http.post<void>(`${this.api}/auth/change-password`, body);
  }
}
