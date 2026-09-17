import { Injectable, computed, inject, signal } from '@angular/core';
import { forkJoin } from 'rxjs';
import { ApiError } from '../api/api-error';
import { ProductsApiService } from '../api/products-api.service';
import { VendingApiService } from '../api/vending-api.service';
import { PurchaseResult, ResetResult, VendingSession } from '../models/coin.model';
import { Product } from '../models/product.model';

const EMPTY_SESSION: VendingSession = { insertedCoins: [], insertedTotalCents: 0 };

/**
 * Owns the vending screen's state. The server is the sole authority on
 * money and eligibility (CLAUDE.md §5.3) — this store only relays what it
 * returns; `canAfford` exists purely to style the Buy button, never to
 * block a purchase attempt.
 *
 * `busy` gates every mutating action (`insertCoin`/`purchase`/`reset`) so a
 * double click can never fire two requests — the most likely real bug on
 * this screen, since it would spend the customer's money twice.
 */
@Injectable({ providedIn: 'root' })
export class VendingStore {
  private readonly productsApi = inject(ProductsApiService);
  private readonly vendingApi = inject(VendingApiService);

  private readonly _products = signal<Product[]>([]);
  private readonly _denominations = signal<number[]>([]);
  private readonly _session = signal<VendingSession>(EMPTY_SESSION);
  private readonly _lastPurchase = signal<PurchaseResult | null>(null);
  private readonly _lastReturn = signal<ResetResult | null>(null);
  private readonly _error = signal<ApiError | null>(null);
  private readonly _loading = signal(false);
  private readonly _busy = signal(false);

  readonly products = this._products.asReadonly();
  readonly denominations = this._denominations.asReadonly();
  readonly session = this._session.asReadonly();
  readonly lastPurchase = this._lastPurchase.asReadonly();
  readonly lastReturn = this._lastReturn.asReadonly();
  readonly error = this._error.asReadonly();
  readonly loading = this._loading.asReadonly();
  readonly busy = this._busy.asReadonly();

  readonly insertedTotal = computed(() => this._session().insertedTotalCents);

  /** UX hint only — the server still validates every purchase attempt. */
  canAfford(productId: string): boolean {
    const product = this._products().find((p) => p.id === productId);
    return product !== undefined && this.insertedTotal() >= product.priceCents;
  }

  load(): void {
    this._loading.set(true);
    this._error.set(null);

    forkJoin({
      products: this.productsApi.list(),
      denominations: this.vendingApi.denominations(),
      session: this.vendingApi.session(),
    }).subscribe({
      next: ({ products, denominations, session }) => {
        this._products.set(products);
        this._denominations.set(denominations);
        this._session.set(session);
        this._loading.set(false);
      },
      error: (apiError: ApiError) => {
        this._error.set(apiError);
        this._loading.set(false);
      },
    });
  }

  insertCoin(denominationCents: number): void {
    if (this._busy()) {
      return;
    }
    this._busy.set(true);
    this._lastPurchase.set(null);
    this._lastReturn.set(null);
    this._error.set(null);

    this.vendingApi.insertCoin(denominationCents).subscribe({
      next: (session) => {
        this._session.set(session);
        this._busy.set(false);
      },
      error: (apiError: ApiError) => {
        this._error.set(apiError);
        this._busy.set(false);
      },
    });
  }

  purchase(productId: string): void {
    if (this._busy()) {
      return;
    }
    this._busy.set(true);

    this.vendingApi.purchase(productId).subscribe({
      next: (result) => {
        this._lastPurchase.set(result);
        this._error.set(null);
        this._products.update((products) =>
          products.map((product) => (product.id === result.product.id ? result.product : product)),
        );
        this._session.set(EMPTY_SESSION);
        this._busy.set(false);
      },
      error: (apiError: ApiError) => {
        this._error.set(apiError);
        this._busy.set(false);
      },
    });
  }

  reset(): void {
    if (this._busy()) {
      return;
    }
    this._busy.set(true);

    this.vendingApi.reset().subscribe({
      next: (result) => {
        this._lastReturn.set(result);
        this._error.set(null);
        this._session.set(EMPTY_SESSION);
        this._busy.set(false);
      },
      error: (apiError: ApiError) => {
        this._error.set(apiError);
        this._busy.set(false);
      },
    });
  }
}
