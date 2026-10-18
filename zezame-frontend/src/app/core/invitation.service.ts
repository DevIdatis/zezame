import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { CreateInvitationRequest, Invitation } from './models';

@Injectable({ providedIn: 'root' })
export class InvitationService {
  private readonly http = inject(HttpClient);
  private readonly api = environment.apiUrl;

  list(): Observable<Invitation[]> {
    return this.http.get<Invitation[]>(`${this.api}/invitations`);
  }

  create(body: CreateInvitationRequest): Observable<Invitation> {
    return this.http.post<Invitation>(`${this.api}/invitations`, body);
  }

  revoke(id: string): Observable<unknown> {
    return this.http.post(`${this.api}/invitations/${id}/revoke`, {});
  }

  resend(id: string): Observable<unknown> {
    return this.http.post(`${this.api}/invitations/${id}/resend`, {});
  }
}
