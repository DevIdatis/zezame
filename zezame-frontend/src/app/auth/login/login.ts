import { Component, inject } from '@angular/core';
import { FormBuilder, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/auth';
import { LoginRequest } from '../../core/models';

@Component({
  selector: 'app-login',
  imports: [FormsModule, ReactiveFormsModule],
  templateUrl: './login.html',
  styleUrl: './login.scss',
})
export class Login {
  private fb = inject(FormBuilder);
  private auth = inject(AuthService);
  private router = inject(Router);

  form = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', Validators.required],
  });

  error = '';

  submit(): void {
    if (this.form.invalid) return;

    this.auth.login(this.form.value as LoginRequest).subscribe({
      next: () => this.router.navigate(['/dashboard']),
      error: (e) => (this.error = e.error || 'Credenciales inválidas'),
    });
  }
}
