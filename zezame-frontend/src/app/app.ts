import { Component, inject, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { AuthService } from './core/auth.service';
import { ErrorModal } from './shared/error-modal/error-modal';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, ErrorModal],
  templateUrl: './app.html',
  styleUrl: './app.scss',
})
export class App {
  private auth = inject(AuthService);
  protected readonly title = signal('Zezame');

  constructor() {
    this.auth.autoLogin();
  }
}
