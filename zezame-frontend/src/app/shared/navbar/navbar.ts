import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { LucideAngularModule, Clock, Menu } from 'lucide-angular';

@Component({
  selector: 'app-navbar',
  imports: [LucideAngularModule],
  templateUrl: './navbar.html',
  styleUrl: './navbar.scss',
})
export class Navbar {
  private router = inject(Router);
  Clock = Clock;
  Menu = Menu;
  isOpen = false;
  showLogin = false;

  constructor() {}

  handleLogin(): void {
    this.showLogin = false;
    this.router.navigate(['/dashboard']);
  }
}
