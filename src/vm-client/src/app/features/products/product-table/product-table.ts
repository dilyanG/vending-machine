import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { CentsToCurrencyPipe } from '../../../core/pipes/cents-to-currency.pipe';
import { Product } from '../../../core/models/product.model';
import { Badge } from '../../../shared/ui/badge/badge';
import { stockBadgeVariant, stockLabel } from '../../../shared/ui/badge/stock-badge';
import { Button } from '../../../shared/ui/button/button';

/**
 * Presentational — never injects ProductsStore. Renders the same data twice
 * (a real <table> and a card list), each hidden from the other by CSS at the
 * md breakpoint, rather than reflowing one table with JS/ARIA role
 * overrides: a horizontally scrolling table on a phone is a failure of the
 * responsive requirement, not a solution to it (CLAUDE.md §5.4).
 */
@Component({
  selector: 'vm-product-table',
  templateUrl: './product-table.html',
  styleUrl: './product-table.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [Badge, Button, CentsToCurrencyPipe],
})
export class ProductTable {
  readonly products = input.required<Product[]>();
  readonly busy = input(false);

  readonly edit = output<Product>();
  readonly remove = output<Product>();

  protected readonly stockBadgeVariant = stockBadgeVariant;
  protected readonly stockLabel = stockLabel;
}
