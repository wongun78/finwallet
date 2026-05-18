import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { AuthService } from '../../core/auth.service';
import { NotificationService } from '../../core/notification.service';

@Component({
  selector: 'app-totp-setup',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MatProgressSpinnerModule],
  templateUrl: './totp.component.html'
})
export class TotpSetupComponent {
  secret = '';
  otpAuthUri = '';
  isLoadingSecret = false;
  isConfirming = false;
  message = '';

  form = this.fb.group({
    code: ['', [Validators.required, Validators.minLength(6)]]
  });

  constructor(
    private fb: FormBuilder,
    private auth: AuthService,
    private notify: NotificationService
  ) {
    this.loadSecret();
  }

  loadSecret(): void {
    this.isLoadingSecret = true;
    this.auth.setupTotp().subscribe({
      next: (result) => {
        this.isLoadingSecret = false;
        this.secret = result.secret;
        this.otpAuthUri = result.otpAuthUri;
      },
      error: () => {
        this.isLoadingSecret = false;
        this.message = 'Khong the lay TOTP secret.';
        this.notify.error(this.message);
      }
    });
  }

  confirm(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isConfirming = true;
    this.message = '';

    this.auth.confirmTotp(this.form.getRawValue().code!).subscribe({
      next: () => {
        this.isConfirming = false;
        this.message = 'Da kich hoat TOTP thanh cong.';
        this.notify.success(this.message);
      },
      error: () => {
        this.isConfirming = false;
        this.message = 'Ma TOTP khong hop le.';
        this.notify.error(this.message);
      }
    });
  }
}
