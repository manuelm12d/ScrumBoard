import { Component, inject } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { AuthService } from '../auth/auth.service';

@Component({
  selector: 'app-topbar',
  standalone: true,
  imports: [RouterLink, ButtonModule],
  template: `
    <header class="topbar">
      <div class="brand">
        <a routerLink="/projects" class="brand-link">
          <span class="brand-mark">SB</span>
          <span class="brand-text">ScrumBoard</span>
        </a>
        <span class="brand-sub">Sakai · PrimeNG</span>
      </div>
      <div class="actions">
        @if (auth.user(); as user) {
          <span class="user">{{ user.name }}</span>
          <button pButton type="button" label="Salir" icon="pi pi-sign-out" class="p-button-text p-button-sm" (click)="logout()"></button>
        }
      </div>
    </header>
  `,
  styles: [
    `
      .topbar {
        height: 4rem;
        display: flex;
        align-items: center;
        justify-content: space-between;
        padding: 0 1.25rem;
        background: var(--surface-card);
        border-bottom: 1px solid var(--surface-border);
        position: sticky;
        top: 0;
        z-index: 100;
      }

      .brand {
        display: flex;
        align-items: baseline;
        gap: 0.75rem;
      }

      .brand-link {
        display: flex;
        align-items: center;
        gap: 0.6rem;
        text-decoration: none;
        color: inherit;
        font-weight: 700;
      }

      .brand-mark {
        width: 2rem;
        height: 2rem;
        border-radius: 0.5rem;
        display: grid;
        place-items: center;
        background: var(--primary-color);
        color: var(--primary-color-text);
        font-size: 0.85rem;
      }

      .brand-sub {
        color: var(--text-color-secondary);
        font-size: 0.8rem;
      }

      .actions {
        display: flex;
        align-items: center;
        gap: 0.75rem;
      }

      .user {
        color: var(--text-color-secondary);
        font-size: 0.9rem;
      }
    `
  ]
})
export class AppTopbarComponent {
  readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  logout(): void {
    this.auth.logout();
    void this.router.navigate(['/login']);
  }
}
