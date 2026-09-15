import { Routes } from '@angular/router';

export const VENDING_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./vending-page').then((m) => m.VendingPage),
  },
];
