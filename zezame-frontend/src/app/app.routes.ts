import { Routes } from '@angular/router';
import { authGuard } from './core/auth.guard';
import { Main } from './main/main';

export const routes: Routes = [
  { path: '', redirectTo: '/main', pathMatch: 'full' },
  { path: 'main', component: Main },
  {
    path: 'dashboard',
    loadComponent: () => import('./dashboard/dashboard').then((m) => m.Dashboard),
    canActivate: [authGuard],
  },
];
