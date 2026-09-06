import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { CreateProjectRequest, Project, UpdateProjectRequest } from './models';

@Injectable({ providedIn: 'root' })
export class ProjectService {
  private http = inject(HttpClient);
  private api = environment.apiUrl;

  list(status?: string): Observable<Project[]> {
    let params = new HttpParams();
    if (status && status !== 'all') params = params.set('status', status);
    return this.http.get<Project[]>(`${this.api}/projects`, { params });
  }

  getById(id: string): Observable<Project> {
    return this.http.get<Project>(`${this.api}/projects/${id}`);
  }

  create(body: CreateProjectRequest): Observable<Project> {
    return this.http.post<Project>(`${this.api}/projects`, body);
  }

  update(id: string, body: UpdateProjectRequest): Observable<Project> {
    return this.http.put<Project>(`${this.api}/projects/${id}`, body);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.api}/projects/${id}`);
  }
}
