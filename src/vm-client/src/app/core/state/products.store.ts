import { Injectable, inject, signal } from '@angular/core';
import { EMPTY, Observable } from 'rxjs';
import { switchMap, tap } from 'rxjs/operators';
import { ApiError } from '../api/api-error';
import { ProductsApiService } from '../api/products-api.service';
import { CreateProductRequest, Product, UpdateProductRequest } from '../models/product.model';

/**
 * Owns the products-admin screen's state. Deliberately separate from
 * vending.store even though both read the same `Product` entity — they have
 * different concerns (CRUD/admin vs. the vending machine's own session), and
 * merging them would couple the admin screen to the machine for no benefit.
 *
 * Mutating methods return the underlying Observable (not auto-subscribed)
 * so the calling page can react to *this specific* submission's outcome —
 * e.g. mapping a DUPLICATE_PRICE error onto the product form's price field —
 * while this store still applies the result to `products` via `tap`, so the
 * list is never refetched after create/update/delete.
 */
@Injectable({ providedIn: 'root' })
export class ProductsStore {
  private readonly productsApi = inject(ProductsApiService);

  private readonly _products = signal<Product[]>([]);
  private readonly _loading = signal(false);
  private readonly _busy = signal(false);
  private readonly _error = signal<ApiError | null>(null);

  readonly products = this._products.asReadonly();
  readonly loading = this._loading.asReadonly();
  readonly busy = this._busy.asReadonly();
  readonly error = this._error.asReadonly();

  load(): void {
    this._loading.set(true);
    this._error.set(null);

    this.productsApi.list().subscribe({
      next: (products) => {
        this._products.set(products);
        this._loading.set(false);
      },
      error: (apiError: ApiError) => {
        this._error.set(apiError);
        this._loading.set(false);
      },
    });
  }

  create(request: CreateProductRequest): Observable<Product> {
    if (this._busy()) {
      return EMPTY;
    }
    this._busy.set(true);
    return this.productsApi.create(request).pipe(
      tap({
        next: (product) => {
          this._products.update((products) => [...products, product]);
          this._error.set(null);
          this._busy.set(false);
        },
        error: (apiError: ApiError) => {
          this._error.set(apiError);
          this._busy.set(false);
        },
      }),
    );
  }

  update(id: string, request: UpdateProductRequest): Observable<Product> {
    if (this._busy()) {
      return EMPTY;
    }
    this._busy.set(true);
    return this.productsApi.update(id, request).pipe(
      tap({
        next: (updated) => {
          this._products.update((products) =>
            products.map((product) => (product.id === id ? updated : product)),
          );
          this._error.set(null);
          this._busy.set(false);
        },
        error: (apiError: ApiError) => {
          this._error.set(apiError);
          this._busy.set(false);
        },
      }),
    );
  }

  remove(id: string): Observable<void> {
    if (this._busy()) {
      return EMPTY;
    }
    this._busy.set(true);
    return this.productsApi.remove(id).pipe(
      tap({
        next: () => {
          this._products.update((products) => products.filter((product) => product.id !== id));
          this._error.set(null);
          this._busy.set(false);
        },
        error: (apiError: ApiError) => {
          this._error.set(apiError);
          this._busy.set(false);
        },
      }),
    );
  }

  /**
   * `POST /api/products/reload` returns 204 (CLAUDE.md's decision log) — there
   * is no response body to apply, so this is the one mutation that genuinely
   * has to refetch the list afterward rather than patch it in place.
   */
  reload(): Observable<Product[]> {
    if (this._busy()) {
      return EMPTY;
    }
    this._busy.set(true);
    return this.productsApi.reload().pipe(
      switchMap(() => this.productsApi.list()),
      tap({
        next: (products) => {
          this._products.set(products);
          this._error.set(null);
          this._busy.set(false);
        },
        error: (apiError: ApiError) => {
          this._error.set(apiError);
          this._busy.set(false);
        },
      }),
    );
  }
}
