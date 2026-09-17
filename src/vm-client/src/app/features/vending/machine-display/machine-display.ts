import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { CentsToCurrencyPipe } from '../../../core/pipes/cents-to-currency.pipe';

/**
 * The vending screen's status area, and the page's single aria-live region —
 * both the inserted total and the current prompt/error live inside it, so an
 * update is announced exactly once. Errors use role="status" like everything
 * else here: none of this machine's messages are urgent interruptions.
 */
@Component({
  selector: 'vm-machine-display',
  templateUrl: './machine-display.html',
  styleUrl: './machine-display.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CentsToCurrencyPipe],
})
export class MachineDisplay {
  readonly insertedTotal = input.required<number>();
  readonly message = input<string | null>(null);
  readonly isError = input(false);
}
