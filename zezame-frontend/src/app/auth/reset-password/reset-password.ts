import { Component, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../../core/auth.service';

@Component({
  selector: 'app-reset-password',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './reset-password.html',
})
export class ResetPassword {
  private route = inject(ActivatedRoute);
  private fb = inject(FormBuilder);
  private auth = inject(AuthService);

  readonly token = this.route.snapshot.queryParamMap.get('token') ?? '';
  state = signal<'form' | 'success'>('form');
  error = signal<string>('');
  submitting = signal(false);

  form = this.fb.group({
    newPassword: ['', [Validators.required, Validators.minLength(8)]],
  });

  submit(): void {
    if (this.form.invalid || !this.token) return;
    this.submitting.set(true);
    this.error.set('');

    this.auth.resetPassword(this.token, this.form.value.newPassword!).subscribe({
      next: () => {
        this.state.set('success');
        this.submitting.set(false);
      },
      error: (e) => {
        this.error.set(
          e?.error?.message ??
            'No se pudo restablecer la contraseña. El enlace puede haber caducado o la contraseña no cumple los requisitos (mínimo 8 caracteres, con mayúscula, minúscula y número).',
        );
        this.submitting.set(false);
      },
    });
  }
}
