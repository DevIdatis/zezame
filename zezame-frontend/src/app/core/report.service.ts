import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { CalendarDay, DaySummary } from './models';

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
}
