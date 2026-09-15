import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'vm-products-page',
  template: `<h1>Products</h1>`,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProductsPage {}
