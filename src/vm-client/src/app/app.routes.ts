import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./features/vending/vending-page').then((m) => m.VendingPage),
  },
  {
    path: 'products',
    loadComponent: () => import('./features/products/products-page').then((m) => m.ProductsPage),
  },
];
