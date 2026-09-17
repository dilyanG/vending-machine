import { ChangeDetectionStrategy, Component, OnInit, computed, inject, signal } from '@angular/core';
import { ApiError } from '../../core/api/api-error';
import { getErrorMessage } from '../../core/api/error-messages';
import { ExternalCatalogApiService } from '../../core/api/external-catalog-api.service';
import { CatalogueProduct, CreateProductRequest, Product } from '../../core/models/product.model';
import { ProductsStore } from '../../core/state/products.store';
import { Button } from '../../shared/ui/button/button';
import { ConfirmDialog } from '../../shared/ui/confirm-dialog/confirm-dialog';
import { EmptyState } from '../../shared/ui/empty-state/empty-state';
import { CentsToCurrencyPipe } from '../../core/pipes/cents-to-currency.pipe';
import { ProductFormDialog } from './product-form-dialog/product-form-dialog';
import { ProductTable } from './product-table/product-table';

/**
 * Wires ProductsStore to the presentational table/form/confirm components.
 * The only component in this feature that injects the store. Also injects
 * ExternalCatalogApiService directly for the read-only catalogue panel —
 * that data is not part of "products state" (it is a separate, never-
 * mutated source, CLAUDE.md §2.6), so it does not belong in ProductsStore.
 */
@Component({
  selector: 'vm-products-page',
  templateUrl: './products-page.html',
  styleUrl: './products-page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ProductTable, ProductFormDialog, ConfirmDialog, EmptyState, Button, CentsToCurrencyPipe],
})
export class ProductsPage implements OnInit {
  protected readonly store = inject(ProductsStore);
  private readonly externalCatalogApi = inject(ExternalCatalogApiService);

  protected readonly formOpen = signal(false);
  protected readonly editingProduct = signal<Product | null>(null);
  protected readonly formError = signal<ApiError | null>(null);

  protected readonly deleteTarget = signal<Product | null>(null);
  protected readonly deleteMessage = computed(() => {
    const target = this.deleteTarget();
    return target
      ? `Delete "${target.name}"? This can't be undone, and its stock will be lost.`
      : '';
  });

  protected readonly reloadConfirmOpen = signal(false);
  protected readonly reloadMessage = signal<string | null>(null);

  protected readonly externalCatalogue = signal<CatalogueProduct[] | null>(null);
  protected readonly externalCatalogueError = signal<string | null>(null);

  ngOnInit(): void {
    this.store.load();
  }

  protected openCreate(): void {
    this.editingProduct.set(null);
    this.formError.set(null);
    this.formOpen.set(true);
  }

  protected openEdit(product: Product): void {
    this.editingProduct.set(product);
    this.formError.set(null);
    this.formOpen.set(true);
  }

  protected onFormClosed(): void {
    this.formOpen.set(false);
    this.formError.set(null);
  }

  protected onSave(request: CreateProductRequest): void {
    const editing = this.editingProduct();
    const result$ = editing ? this.store.update(editing.id, request) : this.store.create(request);
    result$.subscribe({
      next: () => {
        this.formOpen.set(false);
        this.editingProduct.set(null);
        this.formError.set(null);
      },
      error: (apiError: ApiError) => {
        this.formError.set(apiError);
      },
    });
  }

  protected requestDelete(product: Product): void {
    this.deleteTarget.set(product);
  }

  protected onDeleteClosed(): void {
    this.deleteTarget.set(null);
  }

  protected onDeleteConfirmed(): void {
    const target = this.deleteTarget();
    if (!target) {
      return;
    }
    this.store.remove(target.id).subscribe();
  }

  protected requestReload(): void {
    this.reloadMessage.set(null);
    this.reloadConfirmOpen.set(true);
  }

  protected onReloadConfirmClosed(): void {
    this.reloadConfirmOpen.set(false);
  }

  protected onReloadConfirmed(): void {
    this.store.reload().subscribe({
      next: (products) => {
        this.reloadMessage.set(
          `Reloaded from the external catalogue — the machine now holds ${products.length} product${products.length === 1 ? '' : 's'}.`,
        );
      },
      error: (apiError: ApiError) => {
        this.reloadMessage.set(getErrorMessage(apiError.code));
      },
    });
  }

  protected onExternalCatalogueToggle(event: Event): void {
    const details = event.target as HTMLDetailsElement;
    if (!details.open || this.externalCatalogue() !== null) {
      return;
    }
    this.externalCatalogApi.catalogue().subscribe({
      next: (catalogue) => this.externalCatalogue.set(catalogue),
      error: () => this.externalCatalogueError.set('Could not load the external catalogue.'),
    });
  }
}
