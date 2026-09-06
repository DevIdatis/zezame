import { CommonModule } from '@angular/common';
import { Component, computed, EventEmitter, Input, Output } from '@angular/core';
import { LucideAngularModule, X } from 'lucide-angular';

export type BreakdownPeriod = 'day' | 'week' | 'month';

interface TimeEntry {
  id: number;
  project: string;
  task: string;
  color: string;
  start: string;
  end: string;
  pauses: { from: string; to: string }[];
  durationMin: number;
}

@Component({
  selector: 'app-break-down-dialog',
  imports: [CommonModule, LucideAngularModule],
  templateUrl: './break-down-dialog.html',
  styleUrl: './break-down-dialog.scss',
})
export class BreakDownDialog {
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

  dayEntries: TimeEntry[] = [
    {
      id: 1,
      project: 'Diseño Web Zezame',
      task: 'Diseño de landing page',
      color: 'from-coral to-yellow',
      start: '08:15',
      end: '10:30',
      pauses: [
        { from: '09:10', to: '09:25' },
        { from: '09:10', to: '09:25' },
        { from: '09:10', to: '09:25' },
        { from: '09:10', to: '09:25' },
        { from: '09:10', to: '09:25' },
      ],
      durationMin: 120,
    },
  ];

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

  fmt(min: number): string {
    if (min <= 0) return '0m';

    const h = Math.floor(min / 60);
    const m = min % 60;

    return h > 0 ? `${h}h ${String(m).padStart(2, '0')}m` : `${m}m`;
  }

  close() {
    this.openChange.emit(false);
  }

  get totalDayMin() {
    return this.dayEntries.reduce((s, e) => s + e.durationMin, 0);
  }

  get totalDayPauses() {
    return this.dayEntries.reduce((s, e) => s + e.pauses.length, 0);
  }

  pauseMinutes(e: TimeEntry): number {
    return e.pauses.reduce((s, p) => {
      const [fh, fm] = p.from.split(':').map(Number);
      const [th, tm] = p.to.split(':').map(Number);
      return s + (th * 60 + tm) - (fh * 60 + fm);
    }, 0);
  }
}
