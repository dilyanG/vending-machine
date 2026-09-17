import {
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  OnInit,
  computed,
  effect,
  inject,
  signal,
  viewChild,
} from '@angular/core';
import { getErrorMessage } from '../../core/api/error-messages';
import { VendingStore } from '../../core/state/vending.store';
import { Button } from '../../shared/ui/button/button';
import { EmptyState } from '../../shared/ui/empty-state/empty-state';
import { ChangeTray } from './change-tray/change-tray';
import { CoinSlot } from './coin-slot/coin-slot';
import { MachineDisplay } from './machine-display/machine-display';
import { ProductGrid } from './product-grid/product-grid';

const SKELETON_CARDS = [0, 1, 2, 3, 4, 5];

/**
 * Wires vending.store to the presentational components below it — the only
 * component in this feature that injects the store. Also measures the coin
 * panel's height so the sticky mobile bar can reserve exactly that much
 * bottom padding under the product grid, so it never covers the last row.
 */
@Component({
  selector: 'vm-vending-page',
  templateUrl: './vending-page.html',
  styleUrl: './vending-page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [MachineDisplay, CoinSlot, ProductGrid, ChangeTray, EmptyState, Button],
})
export class VendingPage implements OnInit {
  protected readonly store = inject(VendingStore);
  protected readonly skeletonCards = SKELETON_CARDS;

  private readonly panelRef = viewChild<ElementRef<HTMLElement>>('panel');
  protected readonly panelHeight = signal(0);

  protected readonly displayMessage = computed(() => {
    const error = this.store.error();
    return error ? getErrorMessage(error.code) : 'Insert coins, then choose a product.';
  });
  protected readonly isError = computed(() => this.store.error() !== null);
  protected readonly hasInsertedCoins = computed(() => this.store.insertedTotal() > 0);
  protected readonly loadFailed = computed(
    () => this.store.error() !== null && this.store.products().length === 0 && !this.store.loading(),
  );

  constructor() {
    effect((onCleanup) => {
      const el = this.panelRef()?.nativeElement;
      if (!el) {
        return;
      }
      const observer = new ResizeObserver((entries) => {
        const entry = entries[0];
        if (entry) {
          this.panelHeight.set(Math.ceil(entry.contentRect.height));
        }
      });
      observer.observe(el);
      onCleanup(() => observer.disconnect());
    });
  }

  ngOnInit(): void {
    this.store.load();
  }

  protected onInsertCoin(denomination: number): void {
    this.store.insertCoin(denomination);
  }

  protected onBuy(productId: string): void {
    this.store.purchase(productId);
  }

  protected onReturnCoins(): void {
    this.store.reset();
  }
}
