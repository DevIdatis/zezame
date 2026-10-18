import { Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { DatePipe } from '@angular/common';
import { InvitationService } from '../../../core/services/invitation.service';
import { Invitation } from '../../../core/models';

@Component({
  selector: 'app-invitations',
  imports: [ReactiveFormsModule, DatePipe],
  templateUrl: './invitations.html',
})
export class Invitations implements OnInit {
  private svc = inject(InvitationService);
  private fb = inject(FormBuilder);

  invitations = signal<Invitation[]>([]);
  loading = signal(true);
  error = signal('');
  message = signal('');

  form = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    role: ['Employee', [Validators.required]],
  });

  ngOnInit(): void {
    this.load();
  }

  private load(): void {
    this.loading.set(true);
    this.svc.list().subscribe({
      next: (items) => {
        this.invitations.set(items);
        this.loading.set(false);
      },
      error: (e) => {
        this.error.set(
          e?.status === 403
            ? 'Solo los Owner/Admin pueden gestionar invitaciones.'
            : 'No se pudieron cargar las invitaciones.',
        );
        this.loading.set(false);
      },
    });
  }

  create(): void {
    if (this.form.invalid) return;
    this.message.set('');
    this.error.set('');
    this.svc.create({ email: this.form.value.email!, role: this.form.value.role! }).subscribe({
      next: (inv) => {
        this.invitations.set([inv, ...this.invitations()]);
        this.message.set(`Invitación enviada a ${inv.email}.`);
        this.form.reset({ email: '', role: 'Employee' });
      },
      error: (e) => this.error.set(e?.error?.message ?? 'No se pudo crear la invitación.'),
    });
  }

  revoke(inv: Invitation): void {
    this.svc.revoke(inv.id).subscribe({ next: () => this.load() });
  }

  resend(inv: Invitation): void {
    this.svc.resend(inv.id).subscribe({
      next: () => this.message.set(`Invitación reenviada a ${inv.email}.`),
    });
  }
}
