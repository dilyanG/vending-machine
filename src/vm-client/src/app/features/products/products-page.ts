import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'vm-products-page',
  templateUrl: './products-page.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProductsPage {}
