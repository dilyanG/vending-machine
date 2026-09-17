import {
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  computed,
  effect,
  input,
  viewChild,
} from '@angular/core';
import { CentsToCurrencyPipe } from '../../../core/pipes/cents-to-currency.pipe';
import { DenominationLabelPipe } from '../../../core/pipes/denomination-label.pipe';
import { PurchaseResult, ResetResult } from '../../../core/models/coin.model';

/**
 * Shows what just happened: the product dispensed and its change breakdown
 * after a purchase, or the coins given back after a reset. Empty otherwise.
 * Moves focus to itself after a successful purchase so keyboard/screen-reader
 * users are told what happened without having to go looking for it.
 */
@Component({
  selector: 'vm-change-tray',
  templateUrl: './change-tray.html',
  styleUrl: './change-tray.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CentsToCurrencyPipe, DenominationLabelPipe],
})
export class ChangeTray {
  readonly lastPurchase = input<PurchaseResult | null>(null);
  readonly lastReturn = input<ResetResult | null>(null);

  private readonly trayRef = viewChild<ElementRef<HTMLElement>>('tray');

  protected readonly hasContent = computed(() => this.lastPurchase() !== null || this.lastReturn() !== null);

  constructor() {
    effect(() => {
      if (this.lastPurchase() !== null) {
        this.trayRef()?.nativeElement.focus();
      }
    });
  }
}
