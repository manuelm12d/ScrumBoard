import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [RouterLink, RouterLinkActive],
  template: `
    <aside class="sidebar">
      <nav>
        <div class="section-title">Menú</div>
        <a routerLink="/projects" routerLinkActive="active" class="nav-item">
          <i class="pi pi-briefcase"></i>
          <span>Proyectos</span>
        </a>
      </nav>
    </aside>
  `,
  styles: [
    `
      .sidebar {
        width: 16rem;
        background: var(--surface-card);
        border-right: 1px solid var(--surface-border);
        padding: 1.25rem 0.85rem;
      }

      .section-title {
        font-size: 0.75rem;
        text-transform: uppercase;
        letter-spacing: 0.04em;
        color: var(--text-color-secondary);
        margin: 0 0.65rem 0.75rem;
      }

      .nav-item {
        display: flex;
        align-items: center;
        gap: 0.75rem;
        padding: 0.75rem 0.85rem;
        border-radius: 0.65rem;
        color: var(--text-color);
        text-decoration: none;
        transition: background 0.15s ease;
      }

      .nav-item:hover,
      .nav-item.active {
        background: var(--primary-50);
        color: var(--primary-color);
      }
    `
  ]
})
export class AppSidebarComponent {}
