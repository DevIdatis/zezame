import {
  Component,
  EventEmitter,
  Input,
  OnChanges,
  Output,
  SimpleChanges,
  inject,
  signal,
} from '@angular/core';
import { LucideAngularModule, X, Clock } from 'lucide-angular';
import { TimeService } from '../../../../../core/services/time.service';
import { ProjectService } from 'services/project.service';
import { Project } from '../../../../../core/models';

@Component({
  selector: 'app-entry-form-dialog',
  imports: [LucideAngularModule],
  templateUrl: './entry-form-dialog.html',
  styleUrl: './entry-form-dialog.scss',
})
export class EntryFormDialog implements OnChanges {
  private time = inject(TimeService);
  private projectService = inject(ProjectService);

  @Input() open = false;
  @Input() date = ''; // YYYY-MM-DD

  @Output() openChange = new EventEmitter<boolean>();
  @Output() created = new EventEmitter<void>();

  X = X;
  Clock = Clock;

  dateStr = signal('');
  start = signal('09:00');
  end = signal('10:00');
  description = signal('');
  projectId = signal('');
  projects = signal<Project[]>([]);
  saving = signal(false);
  error = signal('');

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['open'] && this.open) {
      this.dateStr.set(this.date || new Date().toISOString().slice(0, 10));
      this.error.set('');
      if (this.projects().length === 0) {
        this.projectService.list().subscribe({
          next: (list) => this.projects.set(list),
          error: () => {},
        });
      }
    }
  }

  submit(): void {
    const d = this.dateStr();
    if (!d) {
      this.error.set('Indica la fecha');
      return;
    }
    const startIso = new Date(`${d}T${this.start()}:00`).toISOString();
    const endIso = new Date(`${d}T${this.end()}:00`).toISOString();
    if (new Date(endIso) <= new Date(startIso)) {
      this.error.set('La hora de fin debe ser posterior al inicio');
      return;
    }

    this.saving.set(true);
    this.error.set('');
    this.time
      .createManual({
        startTime: startIso,
        endTime: endIso,
        description: this.description().trim() || null,
        projectId: this.projectId() || null,
      })
      .subscribe({
        next: () => {
          this.saving.set(false);
          this.created.emit();
          this.close();
        },
        error: (e) => {
          this.saving.set(false);
          this.error.set(e.error?.message ?? 'No se pudo crear la entrada');
        },
      });
  }

  close(): void {
    this.openChange.emit(false);
  }
}
