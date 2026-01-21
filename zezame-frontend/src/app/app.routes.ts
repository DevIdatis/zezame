import { Routes } from '@angular/router';
import { Login } from './auth/login/login';
import { Register } from './auth/register/register';
import { authGuard } from './core/auth-guard';

export const routes: Routes = [
    { path: '', redirectTo: '/dashboard', pathMatch: 'full' },
    { path: 'login',  component: Login },
    { path: 'register', component: Register },
    {
        path: 'dashboard',
        loadComponent: () => import('./dashboard/dashboard')
        .then(m => m.Dashboard),
        canActivate: [authGuard]
    }
];
