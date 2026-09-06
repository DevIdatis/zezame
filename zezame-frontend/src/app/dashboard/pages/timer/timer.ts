import { Component, computed, inject, OnDestroy, OnInit, signal } from '@angular/core';
import {
  LucideAngularModule,
  Clock,
  Play,
  Pause,
  Square,
  Plus,
  Calendar,
  TrendingUp,
  Zap,
  Target,
  ChevronRight,
  Sparkles,
  LucideIconData,
} from 'lucide-angular';
import { TimeService } from '../../../core/time.service';
import { AuthService } from '../../../core/auth.service';
import { BehaviorSubject, distinctUntilChanged, Subject, Subscription, takeUntil } from 'rxjs';
import { EntryType, TimeEntry } from '../../../core/models';
import { AsyncPipe } from '@angular/common';
import { BreakdownPeriod, BreakDownDialog } from './components/break-down-dialog/break-down-dialog';
interface Project {
  id: number;
  name: string;
  color: string;
  hours: number;
  progress: number;
}

interface Activity {
  id: number;
  project: string;
  task: string;
  duration: string;
  time: string;
}

interface Stat {
  label: string;
  value: string;
  icon: LucideIconData;
  color: string;
  period?: BreakdownPeriod;
  emoji: string;
}

type TimerStatus = 'idle' | 'working' | 'paused';

@Component({
  selector: 'app-timer',
  imports: [LucideAngularModule, AsyncPipe, BreakDownDialog],
  templateUrl: './timer.html',
  styleUrl: './timer.scss',
})
export class Timer implements OnInit, OnDestroy {
  private time = inject(TimeService);
  protected auth = inject(AuthService);

  private destroy$ = new Subject<void>();
  private intervalId: number | null = null;

  // ===== STATE =====
  status = signal<TimerStatus>('idle');
  elapsed = signal(0); // segundos sesión actual
  entries = signal<TimeEntry[]>([]);
  todayWorkSeconds = signal(0); // total trabajo cerrado hoy (segundos)

  // ===== DERIVED =====
  isRunning = computed(() => this.status() === 'working');

  workElapsed = computed(() => {
    const currentSession = this.isRunning() ? this.elapsed() : 0;
    return this.todayWorkSeconds() + currentSession; // Sesión actual (work o break)
  });
  // Icons
  icons = {
    Clock,
    Play,
    Pause,
    Square,
    Plus,
    Calendar,
    TrendingUp,
    Zap,
    Target,
    ChevronRight,
    Sparkles,
  };

  // State signals
  isTimerRunning = signal(false);
  timerTime = signal('00:45:23');
  breakdownPeriod = signal<BreakdownPeriod | null>(null);
  currentTask = signal('Diseño de componentes UI');

  // Mock data
  readonly mockProjects: Project[] = [
    // { id: 1, name: 'Diseño Web Zezame', color: 'from-coral to-yellow', hours: 24.5, progress: 75 },
    // { id: 2, name: 'App Mobile React', color: 'from-cyan to-lime', hours: 18.2, progress: 45 },
    // { id: 3, name: 'Dashboard Analytics', color: 'from-yellow to-lime', hours: 12.8, progress: 90 },
    // { id: 4, name: 'API Backend', color: 'from-lime to-cyan', hours: 8.3, progress: 30 },
  ];

  readonly mockRecentActivity: Activity[] = [
    // { id: 1, project: 'Diseño Web Zezame', task: 'Diseño de landing page', duration: '2h 15m', time: 'Hoy, 10:30' },
    // { id: 2, project: 'App Mobile React', task: 'Implementación de auth', duration: '1h 45m', time: 'Hoy, 08:15' },
    // { id: 3, project: 'Dashboard Analytics', task: 'Gráficos de rendimiento', duration: '3h 20m', time: 'Ayer, 14:00' },
    // { id: 4, project: 'API Backend', task: 'Endpoints de usuarios', duration: '2h 00m', time: 'Ayer, 09:30' },
  ];

  // Signals para estadísticas reactivas
  todayHours = signal<string>('Nada');
  weekHours = signal<string>('Nada');
  monthHours = signal<string>('Nada');
  productivity = signal<number>(0);

  // Stats computados automáticamente
  stats = computed<Stat[]>(() => [
    {
      label: 'Hoy',
      value: this.todayHours(),
      icon: Clock,
      color: 'coral',
      period: 'day',
      emoji: '⏱️',
    },
    {
      label: 'Esta semana',
      value: this.weekHours(),
      icon: Calendar,
      color: 'cyan',
      period: 'week',
      emoji: '📅',
    },
    {
      label: 'Este mes',
      value: this.monthHours(),
      icon: TrendingUp,
      color: 'yellow',
      period: 'month',
      emoji: '📈',
    },
    {
      label: 'Productividad',
      value: `${this.productivity()}%`,
      icon: Zap,
      color: 'lime',
      emoji: '⚡',
    },
  ]);

  currentProject = signal(this.mockProjects[0] ?? null);

  ngOnInit(): void {
    this.subscribeToTimer();
    this.loadToday();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    this.clearInterval();
  }

  private subscribeToTimer(): void {
    this.time.current$
      .pipe(
        takeUntil(this.destroy$),
        distinctUntilChanged((a, b) => a?.id === b?.id && a?.endTime === b?.endTime),
      )
      .subscribe((entry) => {
        this.clearInterval();

        if (!entry) {
          if (this.status() !== 'idle') {
            this.status.set('idle');
            this.elapsed.set(0);
            this.loadToday();
          }
          this.isTimerRunning.set(false);
          return;
        }

        if (entry.type === EntryType.Break) {
          this.status.set('paused');
          this.isTimerRunning.set(false);
          return;
        }

        this.status.set('working');
        this.isTimerRunning.set(true);
        this.startTick(entry.startTime);
      });
  }

  private startTick(startTime: string): void {
    this.updateElapsed(startTime);
    this.intervalId = window.setInterval(() => this.updateElapsed(startTime), 1000);
  }

  private updateElapsed(startTime: string): void {
    const seconds = Math.floor((Date.now() - new Date(startTime).getTime()) / 1000);
    this.elapsed.set(seconds);
  }

  private clearInterval(): void {
    if (this.intervalId !== null) {
      clearInterval(this.intervalId);
      this.intervalId = null;
    }
  }

  private loadToday(): void {
    const today = new Date();
    today.setHours(0, 0, 0, 0);

    const tomorrow = new Date(today);
    tomorrow.setDate(tomorrow.getDate() + 1);

    this.time.list(today, tomorrow).subscribe({
      next: (list) => {
        this.entries.set(list);
        this.recalculateTodayWork(list);
      },
      error: (err) => console.error('Error cargando entradas:', err),
    });
  }

  openBreakdown(period?: BreakdownPeriod) {
    if (period) {
      this.breakdownPeriod.set(period);
    }
  }

  closeBreakdown() {
    this.breakdownPeriod.set(null);
  }

  private recalculateTodayWork(list: TimeEntry[]): void {
    let totalSeconds = 0;

    for (const entry of list) {
      if (entry.type !== 0) continue;

      if (entry.endTime) {
        const start = new Date(entry.startTime).getTime();
        const end = new Date(entry.endTime).getTime();
        const diffSeconds = Math.floor((end - start) / 1000);
        totalSeconds += diffSeconds;
      } else {
        const start = new Date(entry.startTime).getTime();
        const now = Date.now();
        totalSeconds += Math.floor((now - start) / 1000);
      }
    }

    this.todayWorkSeconds.set(totalSeconds);
    this.todayHours.set(this.formatDuration(totalSeconds));
  }

  private formatDuration(totalSeconds: number): string {
    if (totalSeconds === 0) return '00:00:00';

    const hours = Math.floor(totalSeconds / 3600);
    const minutes = Math.floor((totalSeconds % 3600) / 60);
    const seconds = totalSeconds % 60;

    const hh = hours.toString().padStart(2, '0');
    const mm = minutes.toString().padStart(2, '0');
    const ss = seconds.toString().padStart(2, '0');

    return `${hh}:${mm}:${ss}`;
  }

  start(): void {
    this.time.start().subscribe();
  }

  pause(): void {
    this.time.pause().subscribe();
  }

  resume(): void {
    this.time.resume().subscribe();
  }

  stop(): void {
    this.time.stop().subscribe({
      next: () => {
        this.status.set('idle');
        this.elapsed.set(0);
        this.clearInterval();
        this.loadToday();
      },
    });
  }

  format(totalSeconds: number): string {
    const h = Math.floor(totalSeconds / 3600)
      .toString()
      .padStart(2, '0');
    const m = Math.floor((totalSeconds % 3600) / 60)
      .toString()
      .padStart(2, '0');
    const s = (totalSeconds % 60).toString().padStart(2, '0');
    return `${h}:${m}:${s}`;
  }
}
