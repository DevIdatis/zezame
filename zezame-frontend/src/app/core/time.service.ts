import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { AuthService } from './auth.service';
import { environment } from '../../environments/environment';
import { BehaviorSubject, Observable } from 'rxjs';
import { TimeEntry } from './models';
import { SignalRService } from './signalR.service';

@Injectable({
  providedIn: 'root',
})
export class TimeService {
  private http = inject(HttpClient);
  private hub = inject(SignalRService);
  private api = environment.apiUrl;

  private running$ = new BehaviorSubject<TimeEntry | null>(null);
  public current$ = this.running$.asObservable();

  constructor() {
    this.loadRunning();

    this.hub.onTimerStarted(() => this.loadRunning());
    this.hub.onTimerStopped(() => this.running$.next(null));
  }

  private loadRunning(): void {
    const { from, to } = this.todayRange();
    this.list(from, to).subscribe((list) => {
      const active = list.find((e) => !e.endTime) ?? null;
      this.running$.next(active);
    });
  }

  list(from: Date, to: Date): Observable<TimeEntry[]> {
    const params = new HttpParams().set('from', from.toISOString()).set('to', to.toISOString());

    return this.http.get<TimeEntry[]>(`${this.api}/timeEntries`, { params });
  }

  start(desc?: string) {
    return this.http.post<void>(`${this.api}/timeEntries/start`, {
      description: desc,
    });
  }

  stop() {
    const id = this.running$.value?.id;
    if (!id) throw new Error('No hay entrada activa');

    return this.http.patch<void>(`${this.api}/timeEntries/stop`, { id });
  }

  private todayRange() {
    const from = new Date();
    from.setHours(0, 0, 0, 0);

    const to = new Date();
    to.setHours(23, 59, 59, 999);

    return { from, to };
  }
}
