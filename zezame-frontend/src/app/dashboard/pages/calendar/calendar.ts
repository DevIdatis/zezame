import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import {
  LucideAngularModule,
  Calendar as CalendarIcon,
  ChevronLeft,
  ChevronRight,
  Clock,
  Plus,
  Filter,
  Target,
  Sparkles,
} from 'lucide-angular';
import { ReportService } from '../../../core/report.service';
import { CalendarDay, DaySummary } from '../../../core/models';
import { EntryFormDialog } from './components/entry-form-dialog/entry-form-dialog';

interface WeekdaySummary {
  day: string;
  hours: number;
  pct: number;
}

@Component({
  selector: 'app-calendar',
  imports: [LucideAngularModule, DecimalPipe, EntryFormDialog],
  templateUrl: './calendar.html',
  styleUrl: './calendar.scss',
})
export class Calendar implements OnInit {
  private reports = inject(ReportService);

  icons = { CalendarIcon, ChevronLeft, ChevronRight, Clock, Plus, Filter, Target, Sparkles };

  readonly weekDays = ['Lun', 'Mar', 'Mié', 'Jue', 'Vie', 'Sáb', 'Dom'];

  private now = new Date();
  year = signal(this.now.getFullYear());
  month = signal(this.now.getMonth() + 1); // 1-12

  days = signal<CalendarDay[]>([]);
  selectedDay = signal(this.now.getDate());
  daySummary = signal<DaySummary | null>(null);

  showEntry = signal(false);

  selectedDateStr = computed(
    () =>
      `${this.year()}-${String(this.month()).padStart(2, '0')}-${String(this.selectedDay()).padStart(2, '0')}`,
  );

  // Etiqueta "Enero 2024"
  monthLabel = computed(() => {
    const d = new Date(this.year(), this.month() - 1, 1);
    const label = d.toLocaleDateString('es-ES', { month: 'long', year: 'numeric' });
    return label.charAt(0).toUpperCase() + label.slice(1);
  });

  // Nº de celdas vacías al inicio (offset del día 1, lunes primero)
  leadingBlanks = computed(() => {
    const first = new Date(this.year(), this.month() - 1, 1);
    return (first.getDay() + 6) % 7; // 0 = lunes
  });

  blanks = computed(() => Array.from({ length: this.leadingBlanks() }));

  isToday = (day: number) =>
    this.year() === this.now.getFullYear() &&
    this.month() === this.now.getMonth() + 1 &&
    day === this.now.getDate();

  isSelectedToday = computed(() => this.isToday(this.selectedDay()));

  // Resumen semanal (Lun-Vie) de la semana que contiene el día seleccionado
  weeklySummary = computed<WeekdaySummary[]>(() => {
    const sel = new Date(this.year(), this.month() - 1, this.selectedDay());
    const dow = (sel.getDay() + 6) % 7; // 0 = lunes
    const monday = new Date(sel);
    monday.setDate(sel.getDate() - dow);

    const labels = ['Lun', 'Mar', 'Mié', 'Jue', 'Vie'];
    return labels.map((label, i) => {
      const d = new Date(monday);
      d.setDate(monday.getDate() + i);
      const hours =
        d.getFullYear() === this.year() && d.getMonth() + 1 === this.month()
          ? (this.days().find((x) => x.day === d.getDate())?.hours ?? 0)
          : 0;
      return { day: label, hours, pct: Math.min(100, Math.round((hours / 8) * 100)) };
    });
  });

  weeklyTotal = computed(() => this.weeklySummary().reduce((s, d) => s + d.hours, 0));

  ngOnInit(): void {
    this.loadMonth();
    this.loadDay(this.selectedDay());
  }

  private monthParam(): string {
    return `${this.year()}-${String(this.month()).padStart(2, '0')}`;
  }

  private dayParam(day: number): string {
    return `${this.year()}-${String(this.month()).padStart(2, '0')}-${String(day).padStart(2, '0')}`;
  }

  private loadMonth(): void {
    this.reports.getCalendar(this.monthParam()).subscribe({
      next: (list) => this.days.set(list),
      error: (err) => console.error('Error cargando calendario:', err),
    });
  }

  private loadDay(day: number): void {
    this.reports.getDay(this.dayParam(day)).subscribe({
      next: (summary) => this.daySummary.set(summary),
      error: (err) => console.error('Error cargando día:', err),
    });
  }

  selectDay(day: number): void {
    this.selectedDay.set(day);
    this.loadDay(day);
  }

  onEntryCreated(): void {
    this.loadMonth();
    this.loadDay(this.selectedDay());
  }

  prevMonth(): void {
    if (this.month() === 1) {
      this.month.set(12);
      this.year.update((y) => y - 1);
    } else {
      this.month.update((m) => m - 1);
    }
    this.selectedDay.set(1);
    this.loadMonth();
    this.loadDay(1);
  }

  nextMonth(): void {
    if (this.month() === 12) {
      this.month.set(1);
      this.year.update((y) => y + 1);
    } else {
      this.month.update((m) => m + 1);
    }
    this.selectedDay.set(1);
    this.loadMonth();
    this.loadDay(1);
  }

  goToday(): void {
    this.now = new Date();
    this.year.set(this.now.getFullYear());
    this.month.set(this.now.getMonth() + 1);
    this.selectedDay.set(this.now.getDate());
    this.loadMonth();
    this.loadDay(this.now.getDate());
  }

  // ===== Colores por intensidad (idéntico a CalendarPage.tsx) =====
  getHoursColor(hours: number): string {
    if (hours === 0) return 'bg-muted/50';
    if (hours < 4) return 'bg-yellow/30';
    if (hours < 6) return 'bg-lime/30';
    if (hours < 8) return 'bg-cyan/30';
    return 'bg-coral/30';
  }

  getHoursIntensity(hours: number): string {
    if (hours === 0) return '';
    if (hours >= 8) return 'ring-2 ring-coral/50';
    if (hours >= 6) return 'ring-2 ring-cyan/50';
    return '';
  }

  // ===== Formato =====
  formatHm(totalMinutes: number): string {
    if (totalMinutes <= 0) return '0m';
    const h = Math.floor(totalMinutes / 60);
    const m = totalMinutes % 60;
    return h > 0 ? `${h}h ${String(m).padStart(2, '0')}m` : `${m}m`;
  }

  formatTimeRange(start: string, end: string | null): string {
    const fmt = (iso: string) =>
      new Date(iso).toLocaleTimeString('es-ES', { hour: '2-digit', minute: '2-digit' });
    return end ? `${fmt(start)} - ${fmt(end)}` : `${fmt(start)} - ...`;
  }
}
