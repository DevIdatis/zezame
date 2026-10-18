import { Component, EventEmitter, Input, Output, inject, signal } from '@angular/core';
import { LucideAngularModule, X, FolderPlus } from 'lucide-angular';
import { ProjectService } from '../../../../../core/project.service';
import { Project } from '../../../../../core/models';

@Component({
  selector: 'app-project-form-dialog',
  imports: [LucideAngularModule],
  templateUrl: './project-form-dialog.html',
  styleUrl: './project-form-dialog.scss',
})
export class ProjectFormDialog {
  private projectService = inject(ProjectService);

  @Input() open = false;
  @Output() openChange = new EventEmitter<boolean>();
  @Output() created = new EventEmitter<Project>();

  X = X;
  FolderPlus = FolderPlus;

  name = signal('');
  description = signal('');
  emoji = signal('');
  color = signal('from-coral to-yellow');
  deadline = signal('');
  saving = signal(false);
  error = signal('');

  readonly colors = [
    'from-coral to-yellow',
    'from-cyan to-lime',
    'from-yellow to-lime',
    'from-lime to-cyan',
    'from-coral to-cyan',
    'from-purple to-coral',
  ];
  readonly emojiOptions = ['🎨', '📱', '📊', '⚙️', '📣', '📚', '🚀', '💡'];

  submit(): void {
    const name = this.name().trim();
    if (!name) {
      this.error.set('El nombre es obligatorio');
      return;
    }
    this.saving.set(true);
    this.error.set('');
    this.projectService
      .create({
        name,
        description: this.description().trim() || null,
        color: this.color(),
        emoji: this.emoji() || null,
        deadline: this.deadline() ? new Date(this.deadline()).toISOString() : null,
      })
      .subscribe({
        next: (project) => {
          this.saving.set(false);
          this.created.emit(project);
          this.reset();
          this.close();
        },
        error: (e) => {
          this.saving.set(false);
          this.error.set(e.error?.message ?? 'No se pudo crear el proyecto');
        },
      });
  }

  private reset(): void {
    this.name.set('');
    this.description.set('');
    this.emoji.set('');
    this.color.set('from-coral to-yellow');
    this.deadline.set('');
  }

  close(): void {
    this.openChange.emit(false);
  }
}
