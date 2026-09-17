import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';
import { DenominationLabelPipe } from '../../../core/pipes/denomination-label.pipe';
import { Button } from '../../../shared/ui/button/button';

/**
 * One button per accepted denomination, always rendered from `denominations`
 * (fetched from the API — CLAUDE.md §2.2 forbids hard-coding this list).
 * Coins are drawn as circles sized in proportion to their value: each button
 * carries a `--coin-t` custom property (0..1, this component's own math) and
 * the actual min/max circle size comes from `_tokens.scss` in coin-slot.scss,
 * so no pixel value lives in this file.
 */
@Component({
  selector: 'vm-coin-slot',
  templateUrl: './coin-slot.html',
  styleUrl: './coin-slot.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [Button, DenominationLabelPipe],
})
export class CoinSlot {
  readonly denominations = input.required<number[]>();
  readonly disabled = input(false);
  readonly hasInsertedCoins = input(false);

  readonly insertCoin = output<number>();
  readonly returnCoins = output<void>();

  private readonly coinRatios = computed(() => {
    const list = this.denominations();
    const ratios = new Map<number, number>();
    if (list.length === 0) {
      return ratios;
    }
    const min = Math.min(...list);
    const max = Math.max(...list);
    for (const denomination of list) {
      const ratio =
        max === min
          ? 1
          : (Math.sqrt(denomination) - Math.sqrt(min)) / (Math.sqrt(max) - Math.sqrt(min));
      ratios.set(denomination, ratio);
    }
    return ratios;
  });

  protected ratioFor(denomination: number): number {
    return this.coinRatios().get(denomination) ?? 0;
  }

  protected ariaLabelFor(denomination: number): string {
    if (denomination < 100) {
      return `Insert ${denomination} cent coin`;
    }
    return `Insert ${denomination / 100} euro coin`;
  }
}
