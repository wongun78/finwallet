import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { CommonModule } from '@angular/common';
import { StorageService } from './core/storage.service';
import { AuthService } from './core/auth.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet],
  templateUrl: './app.component.html'
})
export class AppComponent {
  constructor(private storage: StorageService, private auth: AuthService) {}

  get email(): string | null {
    return this.storage.getEmail();
  }

  get isLoggedIn(): boolean {
    return !!this.storage.getAccessToken();
  }

  logout(): void {
    this.auth.logout().subscribe({
      next: () => this.auth.redirectToLogin(),
      error: () => this.auth.redirectToLogin()
    });
  }
}
