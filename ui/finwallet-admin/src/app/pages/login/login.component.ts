import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { AuthService } from '../../core/auth.service';
import { NotificationService } from '../../core/notification.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink, MatProgressSpinnerModule],
  templateUrl: './login.component.html'
})
export class LoginComponent {
  isLoading = false;
  errorMessage = '';
  requiresTotp = false;

  form = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(8)]],
    totpCode: ['']
  });

  constructor(
    private fb: FormBuilder,
    private auth: AuthService,
    private router: Router,
    private notify: NotificationService
  ) {}

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    const { email, password, totpCode } = this.form.getRawValue();

    this.auth.login(email!, password!, this.requiresTotp ? totpCode ?? '' : undefined).subscribe({
      next: (result) => {
        this.isLoading = false;
        if (result.requiresTotp) {
          this.requiresTotp = true;
          this.errorMessage = 'Can ma TOTP de tiep tuc.';
          this.notify.info('Can ma TOTP de tiep tuc.');
          return;
        }
        this.notify.success('Dang nhap thanh cong.');
        this.router.navigateByUrl('/');
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = err?.error?.detail || 'Dang nhap that bai.';
        this.notify.error(this.errorMessage);
      }
    });
  }
}
