import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges, computed, signal } from '@angular/core';
import { LucideAngularModule, Search, Plus, Check, Folder, Sparkles, X } from 'lucide-angular';

export interface ProjectOption {
  id: number;
  name: string;
  color: string;
}

interface Activity {
  id: number;
  name: string;
  emoji: string;
}

// TODO(Fase 4): sustituir por actividades reales por proyecto (GET /api/activities/by-project/{id})
const MOCK_ACTIVITIES: Record<number, Activity[]> = {
  1: [
    { id: 11, name: 'Diseño de landing page', emoji: '🎨' },
    { id: 12, name: 'Diseño de componentes UI', emoji: '🧩' },
    { id: 13, name: 'Prototipado en Figma', emoji: '✏️' },
    { id: 14, name: 'Revisión con cliente', emoji: '💬' },
  ],
  2: [
    { id: 21, name: 'Implementación de auth', emoji: '🔐' },
    { id: 22, name: 'Navegación y rutas', emoji: '🧭' },
    { id: 23, name: 'Integración API', emoji: '🔌' },
    { id: 24, name: 'Testing en dispositivos', emoji: '📱' },
  ],
  3: [
    { id: 31, name: 'Gráficos de rendimiento', emoji: '📊' },
    { id: 32, name: 'Filtros avanzados', emoji: '🔍' },
    { id: 33, name: 'Exportar reportes', emoji: '📤' },
  ],
  4: [
    { id: 41, name: 'Endpoints de usuarios', emoji: '👥' },
    { id: 42, name: 'Optimización de queries', emoji: '⚡' },
    { id: 43, name: 'Documentación API', emoji: '📚' },
  ],
};

@Component({
  selector: 'app-assign-activity-dialog',
  imports: [LucideAngularModule],
  templateUrl: './assign-activity-dialog.html',
  styleUrl: './assign-activity-dialog.scss',
})
export class AssignActivityDialog implements OnChanges {
  @Input() open = false;
  @Input() projects: ProjectOption[] = [];
  @Input() initialProjectId?: number;

  @Output() openChange = new EventEmitter<boolean>();
  @Output() assign = new EventEmitter<{ projectId: number; activityName: string }>();

  Search = Search;
  Plus = Plus;
  Check = Check;
  Folder = Folder;
  Sparkles = Sparkles;
  X = X;

  selectedProjectId = signal<number | null>(null);
  selectedActivityId = signal<number | null>(null);
  customActivity = signal('');
  search = signal('');

  activities = computed<Activity[]>(() => {
    const id = this.selectedProjectId();
    return id != null ? (MOCK_ACTIVITIES[id] ?? []) : [];
  });

  filtered = computed<Activity[]>(() => {
    const q = this.search().toLowerCase();
    return this.activities().filter((a) => a.name.toLowerCase().includes(q));
  });

  selectedProject = computed<ProjectOption | undefined>(() =>
    this.projects.find((p) => p.id === this.selectedProjectId()),
  );

  ngOnChanges(changes: SimpleChanges): void {
    // Al abrirse, inicializa la selección igual que el diálogo de React
    if (changes['open'] && this.open) {
      this.selectedProjectId.set(this.initialProjectId ?? this.projects[0]?.id ?? null);
      this.selectedActivityId.set(null);
      this.customActivity.set('');
      this.search.set('');
    }
  }

  selectProject(id: number): void {
    this.selectedProjectId.set(id);
    this.selectedActivityId.set(null);
  }

  selectActivity(id: number): void {
    this.selectedActivityId.set(id);
    this.customActivity.set('');
  }

  onSearchInput(value: string): void {
    this.search.set(value);
  }

  onCustomInput(value: string): void {
    this.customActivity.set(value);
    if (value) this.selectedActivityId.set(null);
  }

  get canAssign(): boolean {
    return !!this.customActivity().trim() || this.selectedActivityId() !== null;
  }

  handleAssign(): void {
    const name =
      this.customActivity().trim() ||
      this.activities().find((a) => a.id === this.selectedActivityId())?.name;
    const project = this.selectedProject();
    if (!project || !name) return;

    this.assign.emit({ projectId: project.id, activityName: name });
    this.close();
  }

  close(): void {
    this.openChange.emit(false);
  }
}
