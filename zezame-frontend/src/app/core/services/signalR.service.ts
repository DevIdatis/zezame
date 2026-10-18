import { inject, Injectable } from '@angular/core';
import { AuthService } from 'services/auth.service';
import { Subject } from 'rxjs';
import { environment } from '../../../environments/environment';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';

@Injectable({ providedIn: 'root' })
export class SignalRService {
  private hub: HubConnection;
  private auth = inject(AuthService);

  private start$ = new Subject<void>();
  private pause$ = new Subject<void>();
  private stop$ = new Subject<void>();
  private resume$ = new Subject<void>();

  constructor() {
    this.hub = new HubConnectionBuilder()
      .withUrl(environment.hubUrl, {
        accessTokenFactory: () => this.auth.getToken()!,
      })
      .withAutomaticReconnect()
      .build();

    this.hub.on('TimerStarted', () => this.start$.next());
    this.hub.on('TimerPaused', () => this.pause$.next());
    this.hub.on('TimerStopped', () => this.stop$.next());
    this.hub.on('TimerResumed', () => this.resume$.next());

    this.hub.start().catch(console.error);
  }

  onTimerStarted(fn: () => void) {
    return this.start$.subscribe(fn);
  }

  onTimerStopped(fn: () => void) {
    return this.stop$.subscribe(fn);
  }

  onTimerPaused(fn: () => void) {
    return this.pause$.subscribe(fn);
  }

  onTimerResumed(fn: () => void) {
    return this.resume$.subscribe(fn);
  }
}
