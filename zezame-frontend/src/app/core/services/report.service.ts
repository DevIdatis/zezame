import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  CalendarDay,
  DaySummary,
  ProjectBreakdown,
  ReportSummary,
  TeamMember,
  WeeklyPoint,
} from '../models';

@Injectable({ providedIn: 'root' })
export class ReportService {
  private http = inject(HttpClient);
  private api = environment.apiUrl;

  // month: 'YYYY-MM'
  getCalendar(month: string): Observable<CalendarDay[]> {
    const params = new HttpParams().set('month', month);
    return this.http.get<CalendarDay[]>(`${this.api}/reports/calendar`, { params });
  }

  // date: 'YYYY-MM-DD'
  getDay(date: string): Observable<DaySummary> {
    const params = new HttpParams().set('date', date);
    return this.http.get<DaySummary>(`${this.api}/reports/day`, { params });
  }

  getSummary(month: string): Observable<ReportSummary> {
    const params = new HttpParams().set('month', month);
    return this.http.get<ReportSummary>(`${this.api}/reports/summary`, { params });
  }

  getByProject(month: string): Observable<ProjectBreakdown[]> {
    const params = new HttpParams().set('month', month);
    return this.http.get<ProjectBreakdown[]>(`${this.api}/reports/by-project`, { params });
  }

  getWeekly(month: string): Observable<WeeklyPoint[]> {
    const params = new HttpParams().set('month', month);
    return this.http.get<WeeklyPoint[]>(`${this.api}/reports/weekly`, { params });
  }

  getTeam(month: string): Observable<TeamMember[]> {
    const params = new HttpParams().set('month', month);
    return this.http.get<TeamMember[]>(`${this.api}/reports/team`, { params });
  }
}
