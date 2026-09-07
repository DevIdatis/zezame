import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import {
  LucideAngularModule,
  Target,
  Plus,
  Clock,
  Users,
  MoreVertical,
  Search,
  Grid3X3,
  List,
  Play,
  Calendar,
  CheckCircle2,
  ArrowUpRight,
} from 'lucide-angular';
import { ProjectService } from '../../../core/project.service';
import { Project } from '../../../core/models';

type ViewMode = 'grid' | 'list';
type FilterKey = 'all' | 'active' | 'paused' | 'completed';

@Component({
  selector: 'app-projects',
  imports: [LucideAngularModule, DatePipe],
  templateUrl: './projects.html',
  styleUrl: './projects.scss',
})
export class Projects implements OnInit {
  private projectService = inject(ProjectService);

  icons = {
    Target,
    Plus,
    Clock,
    Users,
    MoreVertical,
    Search,
    Grid3X3,
    List,
    Play,
    Calendar,
    CheckCircle2,
    ArrowUpRight,
  };

  viewMode = signal<ViewMode>('grid');
  filter = signal<FilterKey>('all');
  searchQuery = signal('');
  projects = signal<Project[]>([]);
  loading = signal(false);

  readonly filters: { key: FilterKey; label: string }[] = [
    { key: 'all', label: 'Todos' },
    { key: 'active', label: 'Activos' },
    { key: 'paused', label: 'Pausados' },
    { key: 'completed', label: 'Completados' },
  ];

  filteredProjects = computed(() => {
    const f = this.filter();
    const q = this.searchQuery().trim().toLowerCase();
    return this.projects().filter((p) => {
      const matchesFilter = f === 'all' || p.status.toLowerCase() === f;
      const matchesSearch =
        !q || p.name.toLowerCase().includes(q) || (p.description ?? '').toLowerCase().includes(q);
      return matchesFilter && matchesSearch;
    });
  });

  // Stats derivadas de la lista real
  stats = computed(() => {
    const list = this.projects();
    const totalHours = list.reduce((sum, p) => sum + p.hours, 0);
    return [
      { label: 'Total Proyectos', value: `${list.length}`, color: 'coral', emoji: '📁' },
      {
        label: 'Activos',
        value: `${list.filter((p) => p.status.toLowerCase() === 'active').length}`,
        color: 'lime',
        emoji: '🚀',
      },
      { label: 'Horas Totales', value: `${Math.round(totalHours)}h`, color: 'cyan', emoji: '⏰' },
      {
        label: 'Completados',
        value: `${list.filter((p) => p.status.toLowerCase() === 'completed').length}`,
        color: 'yellow',
        emoji: '✅',
      },
    ];
  });

  ngOnInit(): void {
    this.load();
  }

  private load(): void {
    this.loading.set(true);
    this.projectService.list().subscribe({
      next: (list) => {
        this.projects.set(list);
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Error cargando proyectos:', err);
        this.loading.set(false);
      },
    });
  }

  setViewMode(mode: ViewMode): void {
    this.viewMode.set(mode);
  }

  setFilter(key: FilterKey): void {
    this.filter.set(key);
  }

  onSearchInput(value: string): void {
    this.searchQuery.set(value);
  }

  statusLabel(status: string): string {
    switch (status.toLowerCase()) {
      case 'active':
        return 'Activo';
      case 'paused':
        return 'Pausado';
      case 'completed':
        return 'Completado';
      default:
        return status;
    }
  }
}
