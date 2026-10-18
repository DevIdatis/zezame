import { Component, OnInit, computed, inject, signal } from '@angular/core';
import {
  LucideAngularModule,
  TrendingUp,
  Download,
  Calendar,
  Clock,
  Target,
  Users,
  ArrowUp,
  ArrowDown,
  Minus,
  Filter,
  ChevronDown,
  BarChart3,
  PieChart,
  Activity,
  Zap,
  LucideIconData,
} from 'lucide-angular';
import { ReportService } from '../../../core/report.service';
import {
  CalendarDay,
  ProjectBreakdown,
  ReportSummary,
  TeamMember,
  WeeklyPoint,
} from '../../../core/models';

interface StatCard {
  label: string;
  value: string;
  change: number;
  icon: LucideIconData;
  color: string;
  emoji: string;
  trend: 'up' | 'down';
}

interface PieSlice {
  dasharray: string;
  offset: number;
  stroke: string;
}

// Paleta para tarta/leyenda (clases literales para que Tailwind no las purgue)
const PALETTE = [
  { stroke: 'stroke-coral', dot: 'bg-coral' },
  { stroke: 'stroke-cyan', dot: 'bg-cyan' },
  { stroke: 'stroke-yellow', dot: 'bg-yellow' },
  { stroke: 'stroke-lime', dot: 'bg-lime' },
  { stroke: 'stroke-purple', dot: 'bg-purple' },
];

@Component({
  selector: 'app-reports',
  imports: [LucideAngularModule],
  templateUrl: './reports.html',
  styleUrl: './reports.scss',
})
export class Reports implements OnInit {
  private reports = inject(ReportService);

  icons = {
    TrendingUp,
    Download,
    Calendar,
    Clock,
    Target,
    Users,
    ArrowUp,
    ArrowDown,
    Minus,
    Filter,
    ChevronDown,
    BarChart3,
    PieChart,
    Activity,
    Zap,
  };

  private now = new Date();
  year = this.now.getFullYear();
  month = this.now.getMonth() + 1;

  monthLabel = (() => {
    const d = new Date(this.year, this.month - 1, 1);
    const l = d.toLocaleDateString('es-ES', { month: 'long', year: 'numeric' });
    return l.charAt(0).toUpperCase() + l.slice(1);
  })();

  summary = signal<ReportSummary | null>(null);
  byProject = signal<ProjectBreakdown[]>([]);
  weekly = signal<WeeklyPoint[]>([]);
  team = signal<TeamMember[]>([]);
  daily = signal<CalendarDay[]>([]);

  // ===== Stats =====
  stats = computed<StatCard[]>(() => {
    const s = this.summary();
    const trend = (c: number): 'up' | 'down' => (c >= 0 ? 'up' : 'down');
    return [
      {
        label: 'Horas Totales',
        value: `${s?.totalHours ?? 0}h`,
        change: s?.totalChange ?? 0,
        icon: Clock,
        color: 'coral',
        emoji: '⏱️',
        trend: trend(s?.totalChange ?? 0),
      },
      {
        label: 'Promedio Diario',
        value: `${s?.avgDailyHours ?? 0}h`,
        change: s?.avgChange ?? 0,
        icon: BarChart3,
        color: 'cyan',
        emoji: '📊',
        trend: trend(s?.avgChange ?? 0),
      },
      {
        label: 'Productividad',
        value: `${s?.productivity ?? 0}%`,
        change: s?.productivityChange ?? 0,
        icon: Zap,
        color: 'yellow',
        emoji: '⚡',
        trend: trend(s?.productivityChange ?? 0),
      },
      {
        label: 'Tiempo Focus',
        value: `${s?.focusHours ?? 0}h`,
        change: s?.focusChange ?? 0,
        icon: Target,
        color: 'lime',
        emoji: '🎯',
        trend: trend(s?.focusChange ?? 0),
      },
    ];
  });

  // ===== Pie =====
  slices = computed<PieSlice[]>(() => {
    let prev = 0;
    return this.byProject().map((p, i) => {
      const slice: PieSlice = {
        dasharray: `${p.percentage} ${100 - p.percentage}`,
        offset: -prev,
        stroke: PALETTE[i % PALETTE.length].stroke,
      };
      prev += p.percentage;
      return slice;
    });
  });

  pieTotalHours = computed(
    () => Math.round(this.byProject().reduce((sum, p) => sum + p.hours, 0) * 10) / 10,
  );

  dotClass(i: number): string {
    return PALETTE[i % PALETTE.length].dot;
  }

  // ===== Weekly =====
  weeklyTotal = computed(
    () => Math.round(this.weekly().reduce((s, w) => s + w.hours, 0) * 10) / 10,
  );

  // ===== Daily activity =====
  dailyMax = computed(() => Math.max(1, ...this.daily().map((d) => d.hours)));

  dailyBarHeight(hours: number): number {
    return Math.round((hours / this.dailyMax()) * 100);
  }

  avatarUrl(seed: string): string {
    return `https://api.dicebear.com/7.x/avataaars/svg?seed=${encodeURIComponent(seed)}&backgroundColor=ffdfbf`;
  }

  exportCsv(): void {
    const s = this.summary();
    const rows: string[] = [];
    rows.push(`Reporte Zezame,${this.monthLabel}`);
    rows.push('');
    rows.push('Métrica,Valor');
    rows.push(`Horas totales,${s?.totalHours ?? 0}h`);
    rows.push(`Promedio diario,${s?.avgDailyHours ?? 0}h`);
    rows.push(`Productividad,${s?.productivity ?? 0}%`);
    rows.push(`Tiempo focus,${s?.focusHours ?? 0}h`);
    rows.push('');
    rows.push('Semana,Horas,Objetivo');
    this.weekly().forEach((w) => rows.push(`${w.week},${w.hours},${w.target}`));
    rows.push('');
    rows.push('Proyecto,Horas,Porcentaje');
    this.byProject().forEach((p) => rows.push(`"${p.name}",${p.hours},${p.percentage}%`));

    const csv = '﻿' + rows.join('\n');
    const blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `zezame-reporte-${this.year}-${String(this.month).padStart(2, '0')}.csv`;
    a.click();
    URL.revokeObjectURL(url);
  }

  ngOnInit(): void {
    const month = `${this.year}-${String(this.month).padStart(2, '0')}`;
    this.reports.getSummary(month).subscribe({ next: (s) => this.summary.set(s), error: () => {} });
    this.reports
      .getByProject(month)
      .subscribe({ next: (b) => this.byProject.set(b), error: () => {} });
    this.reports.getWeekly(month).subscribe({ next: (w) => this.weekly.set(w), error: () => {} });
    this.reports.getTeam(month).subscribe({ next: (t) => this.team.set(t), error: () => {} });
    this.reports.getCalendar(month).subscribe({ next: (d) => this.daily.set(d), error: () => {} });
  }
}
