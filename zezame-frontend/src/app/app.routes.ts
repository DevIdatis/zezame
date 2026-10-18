import { Routes } from '@angular/router';
import { authGuard } from './core/auth.guard';
import { Main } from './main/main';
import { NotFound } from './shared/not-found/not-found';

export const routes: Routes = [
  { path: '', redirectTo: '/main', pathMatch: 'full' },

  { path: 'main', component: Main },

  {
    path: 'verify-email',
    loadComponent: () => import('./auth/verify-email/verify-email').then((m) => m.VerifyEmail),
  },
  {
    path: 'reset-password',
    loadComponent: () => import('./auth/reset-password/reset-password').then((m) => m.ResetPassword),
  },

  {
    path: 'dashboard',
    canActivate: [authGuard],
    loadComponent: () => import('./dashboard/dashboard').then((m) => m.Dashboard),
    children: [
      {
        path: '',
        loadComponent: () => import('./dashboard/pages/timer/timer').then((m) => m.Timer),
      },
      {
        path: 'projects',
        loadComponent: () => import('./dashboard/pages/projects/projects').then((m) => m.Projects),
      },
      {
        path: 'calendar',
        loadComponent: () => import('./dashboard/pages/calendar/calendar').then((m) => m.Calendar),
      },
      {
        path: 'reports',
        loadComponent: () => import('./dashboard/pages/reports/reports').then((m) => m.Reports),
      },
      {
        path: 'settings',
        loadComponent: () => import('./dashboard/pages/settings/settings').then((m) => m.Settings),
      },
    ],
  },

  { path: '**', component: NotFound },
];
