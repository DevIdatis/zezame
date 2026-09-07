import { Component, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Router, RouterOutlet } from '@angular/router';
import {
  LucideAngularModule,
  Clock,
  Target,
  Calendar,
  TrendingUp,
  Settings,
  LogOut,
  Bell,
  Search,
  Flame,
  Menu,
  X,
  LucideIconData,
} from 'lucide-angular';
import { AuthService } from '../core/auth.service';
import { User } from '../core/models';

interface NavItem {
  icon: LucideIconData;
  label: string;
  path: string;
  badge?: string;
}

interface UserCard {
  name: string;
  email: string;
  avatar: string;
  streak: number;
  level: string;
}

@Component({
  selector: 'app-dashboard',
  imports: [RouterOutlet, LucideAngularModule],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss',
})
export class Dashboard {
  private auth = inject(AuthService);
  private router = inject(Router);

  // Icons mapping for lucide-angular
  icons = { Clock, Target, Calendar, TrendingUp, Settings, LogOut, Bell, Search, Flame, Menu, X };

  // State
  mobileMenuOpen = signal(false);

  // Usuario autenticado (fuente real)
  private currentUser = signal<User | null>(null);

  constructor() {
    this.auth.currentUser$.pipe(takeUntilDestroyed()).subscribe((u) => this.currentUser.set(u));
  }

  // Placeholders cosméticos (aún sin lógica de negocio: racha/nivel)
  private readonly placeholderStreak = 12;
  private readonly placeholderLevel = 'Pro Timer ⚡';

  // Datos que consume la plantilla, derivados del usuario real
  readonly user = computed<UserCard>(() => {
    const u = this.currentUser();
    const name = u?.fullName ?? 'Usuario';
    // Avatar generado con dicebear a partir del id (opaco), no del nombre/email
    const seed = u?.id ?? name;
    return {
      name,
      email: u?.email ?? '',
      avatar: `https://api.dicebear.com/7.x/avataaars/svg?seed=${encodeURIComponent(seed)}&backgroundColor=ffdfbf`,
      streak: this.placeholderStreak,
      level: this.placeholderLevel,
    };
  });

  readonly firstName = computed(() => this.user().name.split(' ')[0]);

  readonly navItems: NavItem[] = [
    { icon: this.icons.Clock, label: 'Timer', path: '/dashboard' },
    { icon: this.icons.Target, label: 'Proyectos', path: '/dashboard/projects', badge: '4' },
    { icon: this.icons.Calendar, label: 'Calendario', path: '/dashboard/calendar' },
    { icon: this.icons.TrendingUp, label: 'Reportes', path: '/dashboard/reports' },
    { icon: this.icons.Settings, label: 'Ajustes', path: '/dashboard/settings', badge: '1' },
  ];

  isActive(path: string): boolean {
    const currentUrl = this.router.url;
    if (path === '/dashboard') {
      return currentUrl === '/dashboard';
    }
    return currentUrl.startsWith(path);
  }

  navigate(path: string): void {
    this.router.navigate([path]);
    this.mobileMenuOpen.set(false);
  }

  logout(): void {
    this.auth.logout();
  }

  toggleMobileMenu(): void {
    this.mobileMenuOpen.update((v) => !v);
  }
}
