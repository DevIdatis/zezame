import { Component, Input, Output, EventEmitter, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LucideAngularModule, XCircle, X } from 'lucide-angular';
import { Observable } from 'rxjs';
import { ErrorModalService, ErrorMsg } from '../../core/error-modal.service';

@Component({
  selector: 'app-error-modal',
  imports: [CommonModule, LucideAngularModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './error-modal.html',
  styleUrl: './error-modal.scss',
})
export class ErrorModal {
  icons = { XCircle, X };
  state$: Observable<ErrorMsg | null>;

  constructor(private _svc: ErrorModalService) {
    this.state$ = this._svc.state$;
  }

  close(): void {
    this._svc.close();
  }
}
