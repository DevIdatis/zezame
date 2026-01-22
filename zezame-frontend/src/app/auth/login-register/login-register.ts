import { Component, EventEmitter, inject, Input, Output } from '@angular/core';
import { FormBuilder, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { LucideAngularModule, Eye, EyeOff, Lock, Mail, Sparkles, X } from 'lucide-angular';
import { Router } from '@angular/router';
import { AuthService } from '../../core/auth.service';
import { LoginRequest, RegisterRequest } from '../../core/models';
import { AsyncPipe } from '@angular/common';
import { ErrorModalService } from '../../core/error-modal.service';

@Component({
  selector: 'app-login-register',
  imports: [FormsModule, ReactiveFormsModule, LucideAngularModule, AsyncPipe],
  templateUrl: './login-register.html',
  styleUrl: './login-register.scss',
})
export class LoginRegister {
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
  @Output() close = new EventEmitter<void>();
  @Output() login = new EventEmitter<void>();

  error = '';

  form = this.fb.group({
    fullname: [''],
    email: ['', [Validators.required, Validators.email]],
    password: ['', Validators.required],
  });

  isSignUp = false;
  showPassword = false;

  submit() {
    if (this.form.invalid) return;
    if (!this.isSignUp) {
      this.auth.login(this.form.value as LoginRequest).subscribe({
        next: () => this.router.navigate(['/dashboard']),
        error: (e) =>
          this.errorModal.open({
            title: 'Error al guardar',
            message: e.message ?? 'Revisa tu conexión',
          }),
      });
    } else {
      this.auth.register(this.form.value as RegisterRequest).subscribe({
        next: () => this.router.navigate(['/dashboard']),
        error: (e) =>
          this.errorModal.open({
            title: 'Error al guardar',
            message: e.message ?? 'Revisa tu conexión',
          }),
      });
    }
  }

  closeModal() {
    this.form.reset({
      fullname: '',
      email: '',
      password: '',
    });

    this.form.markAsPristine();
    this.form.markAsUntouched();

    this.isSignUp = false;
    this.showPassword = false;
    this.close.emit();
  }

  toggleMode() {
    this.isSignUp = !this.isSignUp;
    const fullnameCtrl = this.form.get('fullname');

    if (this.isSignUp) {
      fullnameCtrl?.setValidators([Validators.required]);
    } else {
      fullnameCtrl?.clearValidators();
      fullnameCtrl?.reset();
    }

    fullnameCtrl?.updateValueAndValidity();
  }
  // private fb = inject(FormBuilder);
  // private auth = inject(AuthService);
  // private router = inject(Router);

  // form = this.fb.group({
  //   email: ['', [Validators.required, Validators.email]],
  //   password: ['', Validators.required],
  // });

  // error = '';

  // submit(): void {
  //   if (this.form.invalid) return;

  //   this.auth.login(this.form.value as LoginRequest).subscribe({
  //     next: () => this.router.navigate(['/dashboard']),
  //     error: (e) => (this.error = e.error || 'Credenciales inválidas'),
  //   });
  // }
}
