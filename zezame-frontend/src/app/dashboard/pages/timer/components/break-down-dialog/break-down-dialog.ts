import { CommonModule } from '@angular/common';
import {
  Component,
  EventEmitter,
  Input,
  OnChanges,
  Output,
  SimpleChanges,
  computed,
  inject,
  signal,
} from '@angular/core';
import { LucideAngularModule, X } from 'lucide-angular';
import { ReportService } from '../../../../../core/services/report.service';
import { DayEntry, DaySummary, ProjectBreakdown, WeeklyPoint } from '../../../../../core/models';

export type BreakdownPeriod = 'day' | 'week' | 'month';

interface WeekDay {
  day: string;
  date: string;
  hours: number;
  events: number;
  netMin: number;
  pct: number;
}

@Component({
  selector: 'app-break-down-dialog',
  imports: [CommonModule, LucideAngularModule],
  templateUrl: './break-down-dialog.html',
  styleUrl: './break-down-dialog.scss',
})
export class BreakDownDialog implements OnChanges {
  private reports = inject(ReportService);

  @Input() open = false;
  @Input() period: BreakdownPeriod | null = null;

  @Output() openChange = new EventEmitter<boolean>();

  X = X;

  private now = new Date();

  todayFormatted = computed(() =>
    new Date().toLocaleDateString('es-ES', { weekday: 'long', day: 'numeric', month: 'long' }),
  );

  monthLabel = computed(() => {
    const l = this.now.toLocaleDateString('es-ES', { month: 'long', year: 'numeric' });
    return l.charAt(0).toUpperCase() + l.slice(1);
  });

  // ===== DÍA =====
  daySummary = signal<DaySummary | null>(null);
  dayWorkEntries = computed<DayEntry[]>(
    () => this.daySummary()?.entries.filter((e) => e.type !== 'Break') ?? [],
  );
  dayBreakCount = computed(
    () => this.daySummary()?.entries.filter((e) => e.type === 'Break').length ?? 0,
  );
  dayTotalMin = computed(() => this.daySummary()?.totalMinutes ?? 0);

  // ===== SEMANA =====
  weekDays = signal<WeekDay[]>([]);
  weekRangeLabel = signal('');
  weekTotalMin = computed(() => this.weekDays().reduce((s, d) => s + d.netMin, 0));
  weekEntries = computed(() => this.weekDays().reduce((s, d) => s + d.events, 0));
  weekDaysWorked = computed(() => this.weekDays().filter((d) => d.netMin > 0).length);
  weekAvgMin = computed(() => {
    const worked = this.weekDaysWorked();
    return worked > 0 ? Math.round(this.weekTotalMin() / worked) : 0;
  });

  // ===== MES =====
  monthWeeks = signal<WeeklyPoint[]>([]);
  monthProjects = signal<ProjectBreakdown[]>([]);
  monthTotalMin = computed(() =>
    Math.round(this.monthWeeks().reduce((s, w) => s + w.hours, 0) * 60),
  );
  monthMaxHours = computed(() => Math.max(1, ...this.monthWeeks().map((w) => w.hours)));

  ngOnChanges(changes: SimpleChanges): void {
    if (!(changes['open'] || changes['period']) || !this.open || !this.period) return;
    if (this.period === 'day') this.loadDay();
    else if (this.period === 'week') this.loadWeek();
    else if (this.period === 'month') this.loadMonth();
  }

  private monthParam(): string {
    return `${this.now.getFullYear()}-${String(this.now.getMonth() + 1).padStart(2, '0')}`;
  }

  private loadDay(): void {
    const d = this.now;
    const date = `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(
      d.getDate(),
    ).padStart(2, '0')}`;
    this.reports.getDay(date).subscribe({
      next: (s) => this.daySummary.set(s),
      error: (e) => console.error(e),
    });
  }

  private loadWeek(): void {
    this.reports.getCalendar(this.monthParam()).subscribe({
      next: (cal) => {
        const labels = ['Lun', 'Mar', 'Mié', 'Jue', 'Vie', 'Sáb', 'Dom'];
        const monthNames = [
          'ene',
          'feb',
          'mar',
          'abr',
          'may',
          'jun',
          'jul',
          'ago',
          'sep',
          'oct',
          'nov',
          'dic',
        ];
        const sel = new Date(this.now);
        const dow = (sel.getDay() + 6) % 7; // 0 = lunes
        const monday = new Date(sel);
        monday.setDate(sel.getDate() - dow);

        const days: WeekDay[] = labels.map((label, i) => {
          const d = new Date(monday);
          d.setDate(monday.getDate() + i);
          const inMonth =
            d.getFullYear() === this.now.getFullYear() && d.getMonth() === this.now.getMonth();
          const hours = inMonth ? (cal.find((c) => c.day === d.getDate())?.hours ?? 0) : 0;
          const events = inMonth ? (cal.find((c) => c.day === d.getDate())?.events ?? 0) : 0;
          return {
            day: label,
            date: `${d.getDate()} ${monthNames[d.getMonth()]}`,
            hours,
            events,
            netMin: Math.round(hours * 60),
            pct: Math.min(100, Math.round((hours / 8) * 100)),
          };
        });

        this.weekDays.set(days);
        const last = new Date(monday);
        last.setDate(monday.getDate() + 6);
        this.weekRangeLabel.set(
          `Semana del ${monday.getDate()} al ${last.getDate()} de ${monthNames[last.getMonth()]}`,
        );
      },
      error: (e) => console.error(e),
    });
  }

  private loadMonth(): void {
    const month = this.monthParam();
    this.reports.getWeekly(month).subscribe({
      next: (w) => this.monthWeeks.set(w),
      error: (e) => console.error(e),
    });
    this.reports.getByProject(month).subscribe({
      next: (p) => this.monthProjects.set(p),
      error: (e) => console.error(e),
    });
  }

  fmt(min: number): string {
    if (min <= 0) return '0m';
    const h = Math.floor(min / 60);
    const m = min % 60;
    return h > 0 ? `${h}h ${String(m).padStart(2, '0')}m` : `${m}m`;
  }

  time(iso: string | null): string {
    if (!iso) return '...';
    return new Date(iso).toLocaleTimeString('es-ES', { hour: '2-digit', minute: '2-digit' });
  }

  close() {
    this.openChange.emit(false);
  }
}
