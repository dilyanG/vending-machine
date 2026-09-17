import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';
import { Badge } from '../../../shared/ui/badge/badge';
import { stockBadgeVariant, stockLabel } from '../../../shared/ui/badge/stock-badge';
import { Button } from '../../../shared/ui/button/button';
import { CentsToCurrencyPipe } from '../../../core/pipes/cents-to-currency.pipe';
import { Product } from '../../../core/models/product.model';

/**
 * Presentational only — a product in, a buy event out. Never injects the
 * store (CLAUDE.md §5.2's smart/dumb split); `insertedTotal` is passed down
 * so it can style (never disable) the Buy button when the customer hasn't
 * inserted enough yet — the server is still the one that decides, so a
 * purchase attempt here is not blocked, only hinted.
 */
@Component({
  selector: 'vm-product-card',
  templateUrl: './product-card.html',
  styleUrl: './product-card.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [Badge, Button, CentsToCurrencyPipe],
})
export class ProductCard {
  readonly product = input.required<Product>();
  readonly insertedTotal = input(0);

  readonly buy = output<string>();

  protected readonly outOfStock = computed(() => this.product().quantity === 0);
  protected readonly affordable = computed(() => this.insertedTotal() >= this.product().priceCents);
  protected readonly shortfallCents = computed(() =>
    Math.max(0, this.product().priceCents - this.insertedTotal()),
  );

  protected readonly stockBadgeVariant = computed(() => stockBadgeVariant(this.product().quantity));
  protected readonly stockLabel = computed(() => stockLabel(this.product().quantity));

  protected readonly reasonId = computed(() => `vm-product-card-reason-${this.product().id}`);

  protected onBuyClick(): void {
    if (this.outOfStock()) {
      return;
    }
    this.buy.emit(this.product().id);
  }
}
