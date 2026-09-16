import { ChangeDetectionStrategy, Component, input } from '@angular/core';

export type BadgeVariant = 'neutral' | 'success' | 'warning' | 'danger';

/**
 * Usage:
 *   <vm-badge variant="success">In stock</vm-badge>
 *   <vm-badge variant="danger">Out of stock</vm-badge>
 */
@Component({
  selector: 'vm-badge',
  templateUrl: './badge.html',
  styleUrl: './badge.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Badge {
  readonly variant = input<BadgeVariant>('neutral');
}
