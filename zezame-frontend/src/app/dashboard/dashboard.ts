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
  UserPlus,
  Building,
  LucideIconData,
} from 'lucide-angular';
import { AuthService } from '../core/services/auth.service';
import { User } from '../core/models';
import { AuthorizationService } from 'services/authorization.service';
import { NotificationBell } from './components/notification-bell/notification-bell';

interface NavItem {
  icon: LucideIconData;
  label: string;
  path: string;
  badge?: string;
  canSee: boolean;
}

interface UserCard {
  name: string;
  email: string;
  avatar: string;
  streak: number;
  level: string;
  organization: string;
}

@Component({
  selector: 'app-dashboard',
  imports: [RouterOutlet, LucideAngularModule, NotificationBell],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss',
})
export class Dashboard {
  private auth = inject(AuthService);
  private router = inject(Router);
  private readonly authorization = inject(AuthorizationService);

  // Icons mapping for lucide-angular
  icons = { Clock, Target, Calendar, TrendingUp, Settings, LogOut, Bell, Search, Flame, Menu, X, UserPlus, Building };

  // State
  mobileMenuOpen = signal(false);

  // Usuario autenticado (fuente real)
  private currentUser = signal<User | null>(null);

  readonly isOwner = computed(() =>
    this.authorization.isOwner()
  );

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
    const organization = u?.organizationName ?? 'Sin organización';
    // Avatar generado con dicebear a partir del id (opaco), no del nombre/email
    const seed = u?.id ?? name;
    return {
      name,
      email: u?.email ?? '',
      avatar: `https://api.dicebear.com/7.x/avataaars/svg?seed=${encodeURIComponent(seed)}&backgroundColor=ffdfbf`,
      streak: this.placeholderStreak,
      level: this.placeholderLevel,
      organization,
    };
  });

  readonly firstName = computed(() => this.user().name.split(' ')[0]);

  readonly navItems: NavItem[] = [
    { icon: this.icons.Clock, label: 'Timer', path: '/dashboard', canSee: true },
    { icon: this.icons.Target, label: 'Proyectos', path: '/dashboard/projects', canSee: true },
    { icon: this.icons.Calendar, label: 'Calendario', path: '/dashboard/calendar', canSee: true },
    { icon: this.icons.TrendingUp, label: 'Reportes', path: '/dashboard/reports', canSee: true },
    { icon: this.icons.UserPlus, label: 'Invitaciones', path: '/dashboard/invitations', canSee: this.isOwner() },
    { icon: this.icons.Settings, label: 'Ajustes', path: '/dashboard/settings', canSee: true },
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
