import { Component, HostListener, OnInit, computed, inject, signal } from '@angular/core';
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
  Trash2,
  AlertTriangle,
  Loader2,
} from 'lucide-angular';
import { ProjectService } from 'services/project.service';
import { Project } from '../../../core/models';
import { ProjectFormDialog } from './components/project-form-dialog/project-form-dialog';
import { AuthorizationService } from 'services/authorization.service';
import { ProjectManageDialog } from './components/project-manage-dialog/project-manage-dialog';

type ViewMode = 'grid' | 'list';
type FilterKey = 'all' | 'active' | 'paused' | 'completed';

@Component({
  selector: 'app-projects',
  imports: [LucideAngularModule, DatePipe, ProjectFormDialog, ProjectManageDialog],
  templateUrl: './projects.html',
  styleUrl: './projects.scss',
})
export class Projects implements OnInit {
  private projectService = inject(ProjectService);
  private authorization = inject(AuthorizationService);
  showCreate = signal(false);
  readonly isOwner = computed(() => this.authorization.isOwner());
  readonly isAdmin = computed(() => this.authorization.isAdmin());
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
    Trash2,
    AlertTriangle,
    Loader2,
  };

  viewMode = signal<ViewMode>('grid');
  filter = signal<FilterKey>('all');
  searchQuery = signal('');
  projects = signal<Project[]>([]);
  loading = signal(false);

  // Menú contextual
  openMenuId = signal<string | null>(null);

  // Confirmación de borrado
  showDeleteDialog = signal(false);
  projectToDelete = signal<Project | null>(null);
  deleting = signal(false);

  // Signals para controlar el modal
  showManageDialog = signal(false);
  selectedProject = signal<Project | null>(null);

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

  onProjectCreated(project: Project): void {
    this.projects.update((list) => [project, ...list]);
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

  // Abrir modal con el proyecto seleccionado
  openManageDialog(project: Project): void {
    this.selectedProject.set(project);
    this.showManageDialog.set(true);
  }

  // Cerrar modal
  closeManageDialog(): void {
    this.showManageDialog.set(false);
    // Opcional: limpiar después de la animación de cierre
    // setTimeout(() => this.selectedProject.set(null), 300);
  }

  // Handler cuando se actualiza el proyecto desde el modal
  onProjectUpdated(updated: Project): void {
    this.load();
  }

  // Abrir/cerrar menú de los 3 puntos
  toggleMenu(projectId: string, event: MouseEvent): void {
    event.stopPropagation();
    this.openMenuId.update((current) => (current === projectId ? null : projectId));
  }

  // Click en "Eliminar" dentro del menú
  askDelete(project: Project): void {
    this.projectToDelete.set(project);
    this.showDeleteDialog.set(true);
    this.openMenuId.set(null);
  }

  // Confirmar borrado
  confirmDelete(): void {
    const project = this.projectToDelete();
    if (!project) return;

    this.deleting.set(true);
    this.projectService.delete(project.id).subscribe({
      next: () => {
        this.deleting.set(false);
        this.showDeleteDialog.set(false);
        this.projectToDelete.set(null);
        this.reloadProjects(); // o actualizar el array local
      },
      error: (err) => {
        console.error('Error eliminando proyecto:', err);
        this.deleting.set(false);
      },
    });
  }

  cancelDelete(): void {
    this.showDeleteDialog.set(false);
    this.projectToDelete.set(null);
  }

  // Recarga la lista (ajusta al nombre real de tu método de carga)
  private reloadProjects(): void {
    this.load();
  }

  // Cerrar menú al hacer click fuera
  @HostListener('document:click')
  onDocumentClick(): void {
    this.openMenuId.set(null);
  }
}
