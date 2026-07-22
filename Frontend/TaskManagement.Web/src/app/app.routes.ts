import { Routes } from '@angular/router';

import { Login } from './features/auth/login/login';
import { Register } from './features/auth/register/register';

import { AppLayout } from './shared/components/app-layout/app-layout';
import { authGuard } from './core/guards/auth-guard';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'login',
    pathMatch: 'full',
  },

  {
    path: 'login',
    component: Login,
  },

  {
    path: 'register',
    component: Register,
  },

  {
    path: '',
    component: AppLayout,
    canActivate: [authGuard],
    children: [
      {
      path: '',
      redirectTo: 'dashboard',
      pathMatch: 'full'
      },
      
      {
        path: 'dashboard',
        loadComponent: () =>
          import('./features/dashboard/dashboard')
            .then((m) => m.Dashboard),
      },

      {
        path: 'tasks',
        loadComponent: () =>
          import('./features/tasks/tasks')
            .then((m) => m.Tasks),
      },

      {
        path: 'categories',
        loadComponent: () =>
          import('./features/categories/categories')
            .then((m) => m.Categories),
      },
    ],
  },

  {
    path: '**',
    redirectTo: 'login',
  },
];