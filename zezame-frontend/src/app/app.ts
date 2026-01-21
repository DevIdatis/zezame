import { Component, inject, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { AuthService } from './core/auth';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  private auth = inject(AuthService);
  protected readonly title = signal('Zezame');

  constructor(){
    this.auth.autoLogin();
  }
}
