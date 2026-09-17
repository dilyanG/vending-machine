import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { Product } from '../../../core/models/product.model';
import { ProductCard } from '../product-card/product-card';

/** Presentational — wires each product into a vm-product-card and forwards its buy event up. */
@Component({
  selector: 'vm-product-grid',
  templateUrl: './product-grid.html',
  styleUrl: './product-grid.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ProductCard],
})
export class ProductGrid {
  readonly products = input.required<Product[]>();
  readonly insertedTotal = input(0);

  readonly buy = output<string>();
}
