import { Component, computed, inject, OnDestroy, OnInit, signal } from '@angular/core';
import { TimeService } from '../core/time.service';
import { BehaviorSubject, distinctUntilChanged, Subject, takeUntil } from 'rxjs';
import { TimeEntry } from '../core/models';
import { AsyncPipe, DatePipe } from '@angular/common';
import { AuthService } from '../core/auth.service';
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

interface NavItem {
  icon: LucideIconData;
  label: string;
  path: string;
  badge?: string;
}

interface User {
  name: string;
  avatar: string;
  email: string;
  streak: number;
  level: string;
}

@Component({
  selector: 'app-dashboard',
  imports: [AsyncPipe, DatePipe, RouterOutlet, LucideAngularModule],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss',
})
export class Dashboard implements OnInit, OnDestroy {
  private auth = inject(AuthService);
  // Icons mapping for lucide-angular
  icons = { Clock, Target, Calendar, TrendingUp, Settings, LogOut, Bell, Search, Flame, Menu, X };

  // State
  mobileMenuOpen = signal(false);
  searchQuery = signal('');

  // Mock data
  readonly user: User = {
    name: 'María García',
    avatar: 'https://api.dicebear.com/7.x/avataaars/svg?seed=Maria&backgroundColor=ffdfbf',
    email: 'maria@zezame.com',
    streak: 12,
    level: 'Pro Timer ⚡',
  };

  readonly navItems: NavItem[] = [
    { icon: this.icons.Clock, label: 'Timer', path: '/dashboard' },
    { icon: this.icons.Target, label: 'Proyectos', path: '/dashboard/projects', badge: '4' },
    { icon: this.icons.Calendar, label: 'Calendario', path: '/dashboard/calendar' },
    { icon: this.icons.TrendingUp, label: 'Reportes', path: '/dashboard/reports' },
    { icon: this.icons.Settings, label: 'Ajustes', path: '/dashboard/settings', badge: '1' },
  ];

  constructor(private router: Router) {}

  readonly firstName = computed(() => this.user.name.split(' ')[0]);

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
    this.router.navigate(['/']);
  }

  toggleMobileMenu(): void {
    this.mobileMenuOpen.update((v) => !v);
  }

  ngOnInit(): void {
    // Inicialización si es necesario
  }

  ngOnDestroy(): void {
    // Limpieza si es necesario
  }
  // private time = inject(TimeService);
  // protected auth = inject(AuthService);
  // private destroy$ = new Subject<void>();

  // entries$ = new BehaviorSubject<TimeEntry[]>([]);
  // running$ = this.time.current$;
  // elapsed = signal(0); // segundos transcurridos

  // private interval = 0;

  // ngOnInit(): void {
  //   this.loadToday();

  //   this.running$.pipe(takeUntil(this.destroy$)).subscribe((entry) => {
  //     clearInterval(this.interval);
  //     if (entry) {
  //       this.tick(entry.startTime);
  //       this.interval = window.setInterval(() => this.tick(entry.startTime), 1000);
  //     } else {
  //       this.elapsed.set(0);
  //     }
  //   });

  //   this.time.current$
  //     .pipe(
  //       takeUntil(this.destroy$),
  //       distinctUntilChanged((a, b) => a?.id === b?.id && a?.endTime === b?.endTime),
  //     )
  //     .subscribe(() => {
  //       this.loadToday();
  //     });
  // }

  // ngOnDestroy(): void {
  //   console.log('🔴 Dashboard destruido, limpiando...');
  //   this.destroy$.next();
  //   this.destroy$.complete();
  //   clearInterval(this.interval);
  //   this.elapsed.set(0);
  // }

  // private tick(start: string): void {
  //   const seconds = Math.floor((Date.now() - new Date(start).getTime()) / 1000);
  //   this.elapsed.set(seconds);
  // }

  // loadToday(): void {
  //   const today = new Date();
  //   today.setHours(0, 0, 0, 0);
  //   const tomorrow = new Date(today);
  //   tomorrow.setDate(tomorrow.getDate() + 1);

  //   this.time.list(today, tomorrow).subscribe({
  //     next: (list) => {
  //       console.log('Entradas cargadas:', list); // ← Debug
  //       this.entries$.next(list);
  //     },
  //     error: (err) => console.error('Error cargando entradas:', err),
  //   });
  // }

  // start(): void {
  //   this.time.start().subscribe({
  //     next: () => console.log('Timer iniciado'),
  //     error: (err) => console.error('Error al iniciar:', err),
  //   });
  // }

  // stop(): void {
  //   this.time.stop().subscribe({
  //     next: () => console.log('Timer parado'),
  //     error: (err) => console.error('Error al parar:', err),
  //   });
  // }

  // format(totalSeconds: number): string {
  //   const h = Math.floor(totalSeconds / 3600)
  //     .toString()
  //     .padStart(2, '0');
  //   const m = Math.floor((totalSeconds % 3600) / 60)
  //     .toString()
  //     .padStart(2, '0');
  //   const s = (totalSeconds % 60).toString().padStart(2, '0');
  //   return `${h}:${m}:${s}`;
  // }

  // totalDay(): string {
  //   const total = this.entries$.value.reduce((sum, e) => sum + (e.durationMin ?? 0), 0);
  //   return this.format(total * 60);
  // }
}
