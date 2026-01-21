import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../../core/auth';
import { Router } from '@angular/router';
import { RegisterRequest } from '../../core/models';

@Component({
  selector: 'app-register',
  imports: [ReactiveFormsModule],
  templateUrl: './register.html',
  styleUrl: './register.scss',
})
export class Register {
  private fb = inject(FormBuilder);
  private auth = inject(AuthService);
  private router = inject(Router);

  form = this.fb.group({
    email:    ['', [Validators.required, Validators.email]],
    password: ['', Validators.required],
    fullname: ['', Validators.required],
  });

  error = '';

  submit(): void {
    if (this.form.invalid) return;

    this.auth.register(this.form.value as RegisterRequest).subscribe({
      next: () => this.router.navigate(['/dashboard']),
      error: e => this.error = e.error || 'Usuario ya registrado'
    });
  }
}
