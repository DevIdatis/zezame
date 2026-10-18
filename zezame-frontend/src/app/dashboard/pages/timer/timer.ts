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
import { TimeService } from 'services/time.service';
import { AuthService } from 'services/auth.service';
import { ProjectService } from 'services/project.service';
import { distinctUntilChanged, Subject, takeUntil } from 'rxjs';
import { EntryType, Project, TimeEntry } from '../../../core/models';
import { BreakdownPeriod, BreakDownDialog } from './components/break-down-dialog/break-down-dialog';
import {
  AssignActivityDialog,
  AssignSelection,
} from './components/assign-activity-dialog/assign-activity-dialog';

interface RecentActivity {
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
  imports: [LucideAngularModule, BreakDownDialog, AssignActivityDialog],
  templateUrl: './timer.html',
  styleUrl: './timer.scss',
})
export class Timer implements OnInit, OnDestroy {
  private time = inject(TimeService);
  protected auth = inject(AuthService);
  private projectService = inject(ProjectService);

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
  currentTask = signal('Sin actividad');
  assignOpen = signal(false);

  // Proyectos reales del usuario (para "Proyectos Activos" y el diálogo de asociación)
  projects = signal<Project[]>([]);
  // Actividad asociada a la próxima sesión (id real o null si es texto libre)
  currentActivity = signal<{ id: string | null; name: string } | null>(null);

  // Proyectos en el formato que espera el diálogo de asociación
  dialogProjects = computed(() =>
    this.projects().map((p) => ({ id: p.id, name: p.name, color: p.color })),
  );

  // Meta del día
  readonly goalHours = 6;
  readonly goalProgress = computed(() => {
    const pct = (this.todayWorkSeconds() / (this.goalHours * 3600)) * 100;
    return Math.min(100, Math.round(pct));
  });
  readonly workedTodayLabel = computed(() => this.formatHm(this.todayWorkSeconds()));
  readonly goalRemainingLabel = computed(() => {
    const remaining = this.goalHours * 3600 - this.todayWorkSeconds();
    return remaining > 0 ? this.formatHm(remaining) : null;
  });

  // TODO(Fase 6): reemplazar por actividad reciente real del backend
  readonly mockRecentActivity: RecentActivity[] = [
    {
      id: 1,
      project: 'Diseño Web Zezame',
      task: 'Diseño de landing page',
      duration: '2h 15m',
      time: 'Hoy, 10:30',
    },
    {
      id: 2,
      project: 'App Mobile React',
      task: 'Implementación de auth',
      duration: '1h 45m',
      time: 'Hoy, 08:15',
    },
    {
      id: 3,
      project: 'Dashboard Analytics',
      task: 'Gráficos de rendimiento',
      duration: '3h 20m',
      time: 'Ayer, 14:00',
    },
    {
      id: 4,
      project: 'API Backend',
      task: 'Endpoints de usuarios',
      duration: '2h 00m',
      time: 'Ayer, 09:30',
    },
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

  currentProject = signal<Project | null>(null);

  ngOnInit(): void {
    this.subscribeToTimer();
    this.loadToday();
    this.loadProjects();
  }

  private loadProjects(): void {
    this.projectService.list().subscribe({
      next: (list) => {
        this.projects.set(list);
        if (!this.currentProject() && list.length > 0) {
          this.currentProject.set(list[0]);
        }
      },
      error: (err) => console.error('Error cargando proyectos:', err),
    });
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
        // Refresca el trabajo cerrado de hoy en cada transición (start/pausa/reanudar/stop)
        this.loadToday();

        if (!entry) {
          this.status.set('idle');
          this.elapsed.set(0);
          this.isTimerRunning.set(false);
          return;
        }

        if (entry.type === EntryType.Break) {
          this.status.set('paused');
          this.isTimerRunning.set(false);
          this.elapsed.set(0);
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
    this.time.listToday().subscribe({
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

  openAssign() {
    this.assignOpen.set(true);
  }

  closeAssign() {
    this.assignOpen.set(false);
  }

  onAssign(event: AssignSelection) {
    const project = this.projects().find((p) => p.id === event.projectId);
    if (project) this.currentProject.set(project);
    this.currentActivity.set({ id: event.activityId, name: event.activityName });
    this.currentTask.set(event.activityName);
  }

  private recalculateTodayWork(list: TimeEntry[]): void {
    let totalSeconds = 0;

    // Solo entradas de trabajo YA CERRADAS. La sesión en curso se suma aparte
    // (workElapsed), así el contador acumula a través de las pausas sin reiniciarse.
    for (const entry of list) {
      if (entry.type !== 0) continue;
      if (!entry.endTime) continue;

      const start = new Date(entry.startTime).getTime();
      const end = new Date(entry.endTime).getTime();
      totalSeconds += Math.floor((end - start) / 1000);
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
    const project = this.currentProject();
    const activity = this.currentActivity();
    this.time.start(activity?.name, project?.id, activity?.id ?? undefined).subscribe();
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

  // Formato "Xh YYm" (para la meta del día)
  formatHm(totalSeconds: number): string {
    const h = Math.floor(totalSeconds / 3600);
    const m = Math.floor((totalSeconds % 3600) / 60);
    return h > 0 ? `${h}h ${m.toString().padStart(2, '0')}m` : `${m}m`;
  }
}
