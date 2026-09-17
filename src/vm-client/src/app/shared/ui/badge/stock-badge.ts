import { BadgeVariant } from './badge';

/** Shared between vm-product-card (vending) and vm-product-table (products admin) so the two never drift on what "low stock" means. */
export const LOW_STOCK_THRESHOLD = 3;

export function stockBadgeVariant(quantity: number): BadgeVariant {
  if (quantity === 0) {
    return 'danger';
  }
  return quantity <= LOW_STOCK_THRESHOLD ? 'warning' : 'success';
}

export function stockLabel(quantity: number): string {
  if (quantity === 0) {
    return 'Out of stock';
  }
  return quantity <= LOW_STOCK_THRESHOLD ? `Only ${quantity} left` : `${quantity} in stock`;
}
