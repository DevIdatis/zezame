import { CommonModule } from '@angular/common';
import { Component, computed, EventEmitter, inject, Input, Output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { LucideAngularModule, X, Loader2, Plus } from 'lucide-angular';
import { Project, User, UserOrgDto } from '../../../../../core/models';
import { ProjectService } from 'services/project.service';
import { UserService } from 'services/user.service';

@Component({
  selector: 'app-project-manage-dialog',
  imports: [CommonModule, FormsModule, LucideAngularModule],
  templateUrl: './project-manage-dialog.html',
  styleUrl: './project-manage-dialog.scss',
})
export class ProjectManageDialog {
  @Input() open = false;
  @Input() project: Project | null = null;
  @Output() openChange = new EventEmitter<boolean>();
  @Output() updated = new EventEmitter<Project>();

  private projectService = inject(ProjectService);
  private userService = inject(UserService);
  icons = { X, Loader2, Plus };
  // Estado editable del proyecto
  selectedStatus = signal<'Active' | 'Paused' | 'Completed'>('Active');
  editableName = signal('');
  editableDescription = signal('');
  members = signal<UserOrgDto[]>([]);
  availableUsers = signal<UserOrgDto[]>([]);
  searchUser = signal('');
  saving = signal(false);

  statuses = [
    { value: 'Active', label: 'Activo', color: 'bg-lime/20 text-lime border-lime/30' },
    { value: 'Paused', label: 'Pausado', color: 'bg-yellow/20 text-yellow border-yellow/30' },
    { value: 'Completed', label: 'Completado', color: 'bg-cyan/20 text-cyan border-cyan/30' },
  ];

  ngOnInit(): void {
    this.loadAvailableUsers();
  }

  ngOnChanges(): void {
    if (this.open && this.project) {
      this.selectedStatus.set(this.project.status as any);
      this.editableName.set(this.project.name);
      this.editableDescription.set(this.project.description || '');
      this.loadMembers();
    }
  }

  // Usuarios filtrados: coinciden con la búsqueda y aún no son miembros
  filteredAvailableUsers = computed(() => {
    const query = this.searchUser().toLowerCase().trim();
    if (!query) return [];

    const memberIds = new Set(this.members().map((m) => m.id));

    return this.availableUsers()
      .filter((u) => !memberIds.has(u.id))
      .filter((u) => (u.fullName ?? '').toLowerCase().includes(query));
  });

  private loadMembers(): void {
    if (!this.project) return;
    this.projectService.getProjectMembers(this.project.id).subscribe({
      next: (members) => this.members.set(members),
      error: (err) => console.error('Error cargando miembros:', err),
    });
  }

  private loadAvailableUsers(): void {
    this.projectService.getMembersOrganization().subscribe({
      next: (users) => this.availableUsers.set(users),
      error: (err) => console.error('Error cargando usuarios org:', err),
    });
  }

  // Cambiar estado
  changeStatus(status: 'Active' | 'Paused' | 'Completed'): void {
    this.selectedStatus.set(status);
  }

  addMember(user: UserOrgDto): void {
    if (!this.project || this.members().some((m) => m.id === user.id)) return;

    this.projectService.addMember(this.project.id, user.id).subscribe({
      next: () => {
        // Actualiza localmente sin recargar (más rápido)
        this.members.update((list) => [...list, user]);
      },
      error: (err) => console.error('Error añadiendo miembro:', err),
    });
  }

  onAddMemberClick(user: UserOrgDto): void {
    this.addMember(user);
    this.searchUser.set('');
  }

  // Quitar miembro
  removeMember(user: UserOrgDto): void {
    if (!this.project) return;

    this.projectService.removeMember(this.project.id, user.id).subscribe({
      next: () => {
        this.members.update((list) => list.filter((m) => m.id !== user.id));
      },
      error: (err) => console.error('Error quitando miembro:', err),
    });
  }

  // Guardar cambios
  save(): void {
    if (!this.project) return;
    this.saving.set(true);

    const payload = {
      name: this.editableName(),
      description: this.editableDescription(),
      status: this.selectedStatus(),
    };

    this.projectService.update(this.project.id, payload).subscribe({
      next: (updated) => {
        this.saving.set(false);
        this.updated.emit(updated);
        this.close();
      },
      error: () => this.saving.set(false),
    });
  }

  close(): void {
    this.openChange.emit(false);
  }
}
