import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

export interface ErrorMsg {
  title?: string;
  message: string;
}

@Injectable({ providedIn: 'root' })
export class ErrorModalService {
  private readonly _state = new BehaviorSubject<ErrorMsg | null>(null);
  readonly state$ = this._state.asObservable();

  open(opts: ErrorMsg): void {
    this._state.next({ title: 'Algo salió mal', ...opts });
  }

  close(): void {
    this._state.next(null);
  }
}
