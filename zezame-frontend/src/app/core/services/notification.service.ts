import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AppNotification } from '../models';

@Injectable({ providedIn: 'root' })
export class NotificationService {
  private http = inject(HttpClient);
  private api = environment.apiUrl;

  list(): Observable<AppNotification[]> {
    return this.http.get<AppNotification[]>(`${this.api}/notifications`);
  }

  unreadCount(): Observable<number> {
    return this.http.get<number>(`${this.api}/notifications/unread-count`);
  }

  markRead(id: string): Observable<void> {
    return this.http.patch<void>(`${this.api}/notifications/${id}/read`, {});
  }

  markAllRead(): Observable<number> {
    return this.http.post<number>(`${this.api}/notifications/read-all`, {});
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.api}/notifications/${id}`);
  }
}
