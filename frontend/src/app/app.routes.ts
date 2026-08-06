import { Routes } from '@angular/router';
import { authGuard } from './core/auth/auth.guard';
import { AppLayoutComponent } from './core/layout/app-layout.component';
import { LoginComponent } from './features/auth/login/login.component';
import { ProjectsListComponent } from './features/projects/projects-list.component';
import { BoardComponent } from './features/board/board.component';

export const routes: Routes = [
  { path: 'login', component: LoginComponent },
  {
    path: '',
    component: AppLayoutComponent,
    canActivate: [authGuard],
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'projects' },
      { path: 'projects', component: ProjectsListComponent },
      { path: 'projects/:id/board', component: BoardComponent }
    ]
  },
  { path: '**', redirectTo: 'projects' }
];
