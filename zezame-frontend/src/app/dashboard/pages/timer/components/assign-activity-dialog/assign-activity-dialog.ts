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
import { LucideAngularModule, Search, Plus, Check, Folder, Sparkles, X } from 'lucide-angular';
import { ActivityService } from '../../../../../core/activity.service';
import { Activity } from '../../../../../core/models';

export interface ProjectOption {
  id: string;
  name: string;
  color: string;
}

export interface AssignSelection {
  projectId: string;
  activityId: string | null;
  activityName: string;
}

@Component({
  selector: 'app-assign-activity-dialog',
  imports: [LucideAngularModule],
  templateUrl: './assign-activity-dialog.html',
  styleUrl: './assign-activity-dialog.scss',
})
export class AssignActivityDialog implements OnChanges {
  private activityService = inject(ActivityService);

  @Input() open = false;
  @Input() projects: ProjectOption[] = [];
  @Input() initialProjectId?: string;

  @Output() openChange = new EventEmitter<boolean>();
  @Output() assign = new EventEmitter<AssignSelection>();

  Search = Search;
  Plus = Plus;
  Check = Check;
  Folder = Folder;
  Sparkles = Sparkles;
  X = X;

  selectedProjectId = signal<string | null>(null);
  selectedActivityId = signal<string | null>(null);
  customActivity = signal('');
  search = signal('');

  activities = signal<Activity[]>([]);
  loadingActivities = signal(false);

  filtered = computed<Activity[]>(() => {
    const q = this.search().toLowerCase();
    return this.activities().filter((a) => a.name.toLowerCase().includes(q));
  });

  selectedProject = computed<ProjectOption | undefined>(() =>
    this.projects.find((p) => p.id === this.selectedProjectId()),
  );

  ngOnChanges(changes: SimpleChanges): void {
    // Al abrirse, inicializa la selección y carga actividades del proyecto
    if (changes['open'] && this.open) {
      const projectId = this.initialProjectId ?? this.projects[0]?.id ?? null;
      this.selectedActivityId.set(null);
      this.customActivity.set('');
      this.search.set('');
      this.setProject(projectId);
    }
  }

  selectProject(id: string): void {
    this.selectedActivityId.set(null);
    this.setProject(id);
  }

  private setProject(id: string | null): void {
    this.selectedProjectId.set(id);
    this.activities.set([]);
    if (!id) return;

    this.loadingActivities.set(true);
    this.activityService.byProject(id).subscribe({
      next: (list) => {
        this.activities.set(list);
        this.loadingActivities.set(false);
      },
      error: (err) => {
        console.error('Error cargando actividades:', err);
        this.loadingActivities.set(false);
      },
    });
  }

  selectActivity(id: string): void {
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
    const project = this.selectedProject();
    if (!project) return;

    const custom = this.customActivity().trim();
    let activityId: string | null = null;
    let activityName = custom;

    if (!custom) {
      const selected = this.activities().find((a) => a.id === this.selectedActivityId());
      if (!selected) return;
      activityId = selected.id;
      activityName = selected.name;
    }

    this.assign.emit({ projectId: project.id, activityId, activityName });
    this.close();
  }

  close(): void {
    this.openChange.emit(false);
  }
}
