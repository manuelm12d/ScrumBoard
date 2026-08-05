import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { AppTopbarComponent } from './app-topbar.component';
import { AppSidebarComponent } from './app-sidebar.component';

@Component({
  selector: 'app-layout',
  standalone: true,
  imports: [RouterOutlet, AppTopbarComponent, AppSidebarComponent],
  template: `
    <div class="layout-wrapper">
      <app-topbar />
      <div class="layout-main">
        <app-sidebar />
        <div class="layout-content">
          <router-outlet />
        </div>
      </div>
    </div>
  `,
  styles: [
    `
      .layout-wrapper {
        min-height: 100vh;
        background: var(--surface-ground);
        color: var(--text-color);
      }

      .layout-main {
        display: flex;
        min-height: calc(100vh - 4rem);
      }

      .layout-content {
        flex: 1;
        padding: 1.5rem;
        overflow: auto;
      }
    `
  ]
})
export class AppLayoutComponent {}
