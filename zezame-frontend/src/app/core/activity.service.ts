import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Activity, CreateActivityRequest } from './models';

@Injectable({ providedIn: 'root' })
export class ActivityService {
  private http = inject(HttpClient);
  private api = environment.apiUrl;

  byProject(projectId: string): Observable<Activity[]> {
    return this.http.get<Activity[]>(`${this.api}/activities/by-project/${projectId}`);
  }

  create(body: CreateActivityRequest): Observable<Activity> {
    return this.http.post<Activity>(`${this.api}/activities`, body);
  }
}
