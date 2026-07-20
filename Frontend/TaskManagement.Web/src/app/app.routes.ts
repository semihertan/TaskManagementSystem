import { Routes } from '@angular/router';

import { Login } from './features/auth/login/login';
import { Register } from './features/auth/register/register';
import { Tasks } from './features/tasks/tasks';

import { authGuard } from './core/guards/auth-guard';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'login',
    pathMatch: 'full'
  },

  {
    path: 'login',
    component: Login
  },

  {
    path: 'register',
    component: Register
  },

  {
    path: 'tasks',
    canActivate: [authGuard],
    loadComponent: ()=>
      import('./features/tasks/tasks').then(m => m.Tasks)
  },

  {
    path: '**',
    redirectTo: 'login'
  }
];