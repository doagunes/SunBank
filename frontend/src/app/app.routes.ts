import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./login/login').then(m => m.LoginComponent),
  },
  {
    path: 'signup',
    loadComponent: () => import('./signup/signup').then(m => m.SignupComponent),
  },
  {
    path: 'forgot-password',
    loadComponent: () => import('./forgot-password/forgot-password').then(m => m.ForgotPasswordComponent),
  },
  
  {
    path: 'reset-password',
    loadComponent: () => import('./reset-password/reset-password').then(m => m.ResetPasswordComponent),
  },

  {
    path: 'chat',
    loadComponent: () => import('./chatbot/chatbot').then(m => m.ChatbotComponent),
  },
  
  {
    path: 'home',
    loadComponent: () => import('./home/home').then(m => m.HomeComponent),
  },
  {
    path: 'dashboard',
    loadComponent: () => import('./dashboard/dashboard').then(m => m.DashboardComponent), 
  },

  {
    path: 'my-accounts',
    loadComponent: () => import('./my-accounts/my-accounts').then(m => m.MyAccountsComponent)
  },

  {
    path: 'transfer-money',
    loadComponent: () => import('./transfer-money/transfer-money').then(m => m.TransferMoneyComponent)
  },
  
  {
    path: 'bills',
    loadComponent: () => import('./bills/bills').then(m => m.BillsComponent)
  },

  {
    path: '',
    redirectTo: 'home',
    pathMatch: 'full',
  },
]; 