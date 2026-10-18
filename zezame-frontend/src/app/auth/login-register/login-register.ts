import { Component, EventEmitter, inject, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { FormBuilder, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { LucideAngularModule, Eye, EyeOff, Lock, Mail, Sparkles, X } from 'lucide-angular';
import { Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { LoginRequest, RegisterRequest } from '../../core/models';
import { AsyncPipe } from '@angular/common';
import { ErrorModalService } from '../../core/services/error-modal.service';

@Component({
  selector: 'app-login-register',
  imports: [FormsModule, ReactiveFormsModule, LucideAngularModule, AsyncPipe],
  templateUrl: './login-register.html',
  styleUrl: './login-register.scss',
})
export class LoginRegister implements OnChanges{
  private fb = inject(FormBuilder);
  protected auth = inject(AuthService);
  private router = inject(Router);
  private errorModal = inject(ErrorModalService);

  Eye = Eye;
  EyeOff = EyeOff;
  Lock = Lock;
  Mail = Mail;
  Sparkles = Sparkles;
  X = X;

  @Input() isOpen = false;
  @Input() isSignUp = false;
  @Output() close = new EventEmitter<void>();
  @Output() login = new EventEmitter<void>();

  error = '';

  form = this.fb.group({
    fullname: [''],
    organizationName: [''],
    email: ['', [Validators.required, Validators.email]],
    password: ['', Validators.required],
  });

  showPassword = false;
  resetMsg = '';

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['isSignUp']) {
      this.updateValidators();
    }
  }

  requestPasswordReset() {
    const email = this.form.get('email')?.value;
    if (!email || this.form.get('email')?.invalid) {
      this.resetMsg = 'Escribe un email válido arriba y vuelve a pulsar.';
      return;
    }
    // Respuesta neutra: el backend no revela si el email existe.
    this.auth.forgotPassword(email).subscribe({ next: () => {}, error: () => {} });
    this.resetMsg = 'Si ese email tiene cuenta, te hemos enviado instrucciones para restablecer la contraseña.';
  }

  submit() {
    if (this.form.invalid) return;
    if (!this.isSignUp) {
      this.auth.login(this.form.value as LoginRequest).subscribe({
        next: () => this.router.navigate(['/dashboard']),
        error: (e) =>
          this.errorModal.open({
            title: 'Error al realizar el login',
            message: e.error.message ?? 'Revisa tu conexión',
          }),
      });
    } else {
      this.auth.register(this.form.value as RegisterRequest).subscribe({
        next: () => this.router.navigate(['/dashboard']),
        error: (e) =>
          this.errorModal.open({
            title: 'Error al realizar el registro',
            message: e.error.message ?? 'Revisa tu conexión',
          }),
      });
    }
  }

  closeModal(): void {
    this.form.reset({
      fullname: '',
      organizationName: '',
      email: '',
      password: '',
    });

    this.form.markAsPristine();
    this.form.markAsUntouched();

    this.showPassword = false;
    this.resetMsg = '';

    this.close.emit();
  }

  toggleMode() {
    this.isSignUp = !this.isSignUp;
    this.updateValidators();
  }

  private updateValidators(): void {
    const requiredFields = ['fullname', 'organizationName'];

    for (const name of requiredFields) {
      const control = this.form.get(name);

      if (this.isSignUp) {
        control?.setValidators([Validators.required]);
      } else {
        control?.clearValidators();
        control?.reset();
      }

      control?.updateValueAndValidity();
    }
  }
}
