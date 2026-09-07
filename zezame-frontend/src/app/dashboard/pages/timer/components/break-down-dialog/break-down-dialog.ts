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
import { ReportService } from '../../../../../core/report.service';
import { DayEntry, DaySummary } from '../../../../../core/models';

export type BreakdownPeriod = 'day' | 'week' | 'month';

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

  todayFormatted = computed(() => {
    return new Date().toLocaleDateString('es-ES', {
      weekday: 'long',
      day: 'numeric',
      month: 'long',
    });
  });

  // ===== DÍA (datos reales) =====
  daySummary = signal<DaySummary | null>(null);

  dayWorkEntries = computed<DayEntry[]>(
    () => this.daySummary()?.entries.filter((e) => e.type !== 'Break') ?? [],
  );
  dayBreakCount = computed(
    () => this.daySummary()?.entries.filter((e) => e.type === 'Break').length ?? 0,
  );
  dayTotalMin = computed(() => this.daySummary()?.totalMinutes ?? 0);

  ngOnChanges(changes: SimpleChanges): void {
    if ((changes['open'] || changes['period']) && this.open && this.period === 'day') {
      this.loadToday();
    }
  }

  private loadToday(): void {
    const now = new Date();
    const date = `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, '0')}-${String(
      now.getDate(),
    ).padStart(2, '0')}`;
    this.reports.getDay(date).subscribe({
      next: (summary) => this.daySummary.set(summary),
      error: (err) => console.error('Error cargando resumen del día:', err),
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

  // ===== SEMANA / MES (mock — TODO Fase 6) =====
  weekDays = [
    {
      day: 'Lun',
      date: '12 may',
      entries: 4,
      pauses: 3,
      netMin: 390,
      label: '6h 30m',
    },
  ];

  monthWeeks = [
    {
      label: 'Semana 1',
      range: '1 - 7 may',
      entries: 18,
      pauses: 11,
      netMin: 1930,
      hours: '32h 10m',
      pct: 26,
      top: 'Diseño Web Zezame',
    },
  ];

  monthProjects = [
    {
      name: 'Diseño Web Zezame',
      entries: 22,
      hours: '42h 20m',
      color: 'from-coral to-yellow',
    },
  ];
}
