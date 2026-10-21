import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { LucideAngularModule, Clock, Menu } from 'lucide-angular';
import { LoginRegister } from '../../auth/login-register/login-register';

@Component({
  selector: 'app-navbar',
  imports: [LucideAngularModule, LoginRegister],
  templateUrl: './navbar.html',
  styleUrl: './navbar.scss',
})
export class Navbar {
  private router = inject(Router);
  Clock = Clock;
  Menu = Menu;
  isOpen = false;
  signUp = false;
  showLogin = false;

  constructor() {}

  handleLogin(): void {
    this.showLogin = false;
    this.router.navigate(['/dashboard']);
  }

  openLogin(): void {
    this.showLogin = true;
    this.signUp = false;
  }

  openSignUp(): void {
    this.showLogin = true;
    this.signUp = true;
  }
}
