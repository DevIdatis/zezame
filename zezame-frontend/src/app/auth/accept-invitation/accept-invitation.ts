import { Component, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../../core/auth.service';

@Component({
  selector: 'app-accept-invitation',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './accept-invitation.html',
})
export class AcceptInvitation {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private fb = inject(FormBuilder);
  private auth = inject(AuthService);

  readonly token = this.route.snapshot.queryParamMap.get('token') ?? '';
  error = signal('');
  submitting = signal(false);

  form = this.fb.group({
    fullName: ['', [Validators.required]],
    password: ['', [Validators.required, Validators.minLength(8)]],
  });

  submit(): void {
    if (this.form.invalid || !this.token) return;
    this.submitting.set(true);
    this.error.set('');

    this.auth
      .acceptInvitation(this.token, this.form.value.fullName!, this.form.value.password!)
      .subscribe({
        next: () => this.router.navigate(['/dashboard']),
        error: (e) => {
          this.error.set(
            e?.error?.message ??
              'No se pudo aceptar la invitación. El enlace puede haber caducado o la contraseña no cumple los requisitos.',
          );
          this.submitting.set(false);
        },
      });
  }
}
