import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { BehaviorSubject, Observable, tap, shareReplay } from 'rxjs';
import { TimeEntry } from './models';
import { SignalRService } from './signalR.service';

@Injectable({ providedIn: 'root' })
export class TimeService {
  private http = inject(HttpClient);
  private hub = inject(SignalRService);
  private api = environment.apiUrl;

  private listCache = new Map<string, Observable<TimeEntry[]>>();

  // ✅ ÚNICA fuente de verdad
  private currentSubject = new BehaviorSubject<TimeEntry | null>(null);
  current$ = this.currentSubject.asObservable();

  constructor() {
    this.syncCurrent();
    this.hub.onTimerStarted(() => this.syncCurrent());
    this.hub.onTimerPaused(() => this.syncCurrent());
    this.hub.onTimerResumed(() => this.syncCurrent());
    this.hub.onTimerStopped(() => this.currentSubject.next(null));
  }

  // ===============================
  // SYNC
  // ===============================

  private syncCurrent(): void {
    const { from, to } = this.todayRange();

    const params = new HttpParams().set('from', from.toISOString()).set('to', to.toISOString());

    this.http.get<TimeEntry[]>(`${this.api}/timeEntries`, { params }).subscribe((list) => {
      const active = list.find((e) => !e.endTime) ?? null;
      this.currentSubject.next(active);
    });
  }

  // ===============================
  // API
  // ===============================

  // Entradas de hoy usando el MISMO rango que invalidateTodayCache,
  // para que la caché se invalide correctamente en cada transición.
  listToday(): Observable<TimeEntry[]> {
    const { from, to } = this.todayRange();
    return this.list(from, to);
  }

  list(from: Date, to: Date): Observable<TimeEntry[]> {
    const key = `${from.toISOString()}_${to.toISOString()}`;

    if (!this.listCache.has(key)) {
      const params = new HttpParams().set('from', from.toISOString()).set('to', to.toISOString());

      this.listCache.set(
        key,
        this.http.get<TimeEntry[]>(`${this.api}/timeEntries`, { params }).pipe(shareReplay(1)),
      );
    }

    return this.listCache.get(key)!;
  }

  // Crea una entrada manual (con inicio y fin explícitos) — usado en el calendario
  createManual(body: {
    startTime: string;
    endTime: string;
    description?: string | null;
    projectId?: string | null;
    activityId?: string | null;
  }): Observable<TimeEntry> {
    this.invalidateTodayCache();
    return this.http.post<TimeEntry>(`${this.api}/timeEntries`, body);
  }

  start(description?: string, projectId?: string, activityId?: string): Observable<void> {
    this.invalidateTodayCache();
    return this.http
      .post<void>(`${this.api}/timeEntries/start`, { description, projectId, activityId })
      .pipe(tap(() => this.syncCurrent()));
  }

  pause(): Observable<TimeEntry> {
    this.invalidateTodayCache();
    return this.http
      .post<TimeEntry>(`${this.api}/timeEntries/pause`, {})
      .pipe(tap((entry) => this.currentSubject.next(entry)));
  }

  resume(): Observable<TimeEntry> {
    this.invalidateTodayCache();
    return this.http
      .post<TimeEntry>(`${this.api}/timeEntries/resume`, {})
      .pipe(tap((entry) => this.currentSubject.next(entry)));
  }

  stop(): Observable<void> {
    this.invalidateTodayCache();
    const id = this.currentSubject.value?.id;
    if (!id) throw new Error('No hay entrada activa');

    return this.http
      .patch<void>(`${this.api}/timeEntries/stop`, { id })
      .pipe(tap(() => this.currentSubject.next(null)));
  }

  // ===============================
  // HELPERS
  // ===============================

  private todayRange() {
    const from = new Date();
    from.setHours(0, 0, 0, 0);

    const to = new Date();
    to.setHours(23, 59, 59, 999);

    return { from, to };
  }

  private invalidateTodayCache() {
    const { from, to } = this.todayRange();
    const key = `${from.toISOString()}_${to.toISOString()}`;
    this.listCache.delete(key);
  }
}
