import { ChangeDetectionStrategy, Component, computed, effect, inject, input, output } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, ValidatorFn, Validators } from '@angular/forms';
import { ApiError, ErrorCode } from '../../../core/api/api-error';
import { getErrorMessage } from '../../../core/api/error-messages';
import { CreateProductRequest, Product } from '../../../core/models/product.model';
import { Button } from '../../../shared/ui/button/button';
import { Modal } from '../../../shared/ui/modal/modal';
import {
  MAX_QUANTITY_PER_PRODUCT,
  NAME_MAX_LENGTH,
  centsToEuroString,
  euroPriceValidator,
  euroStringToCents,
  integerValidator,
  priceNotDuplicateValidator,
  requiredTrimmedValidator,
} from './product-form.validators';

/** Server error codes this dialog can attach directly to a field; anything else falls back to a dialog-level message. */
const FIELD_ERROR_CODES: readonly ErrorCode[] = [
  'DUPLICATE_PRODUCT',
  'DUPLICATE_PRICE',
  'INVALID_PRICE',
  'INVALID_QUANTITY',
];

/**
 * Create/edit form for a product, in a vm-modal. Never injects
 * ProductsStore — the page owns the store and passes down what this dialog
 * needs (the product to edit, the sibling list for the client-side duplicate
 * -price hint, and the outcome of the last submit attempt), then listens for
 * `save`/`closed`.
 */
@Component({
  selector: 'vm-product-form-dialog',
  templateUrl: './product-form-dialog.html',
  styleUrl: './product-form-dialog.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [Modal, Button, ReactiveFormsModule],
})
export class ProductFormDialog {
  readonly open = input(false);
  readonly product = input<Product | null>(null);
  readonly existingProducts = input<Product[]>([]);
  readonly serverError = input<ApiError | null>(null);
  readonly saving = input(false);

  readonly save = output<CreateProductRequest>();
  readonly closed = output<void>();

  protected readonly MAX_QUANTITY_PER_PRODUCT = MAX_QUANTITY_PER_PRODUCT;
  protected readonly NAME_MAX_LENGTH = NAME_MAX_LENGTH;

  private readonly fb = inject(FormBuilder);

  protected readonly form = this.fb.nonNullable.group({
    name: ['', [requiredTrimmedValidator(), Validators.maxLength(NAME_MAX_LENGTH)]],
    priceEuros: ['', [euroPriceValidator(), this.priceNotDuplicate()]],
    quantity: [
      0,
      [Validators.required, integerValidator(), Validators.min(0), Validators.max(MAX_QUANTITY_PER_PRODUCT)],
    ],
    imageUrl: [''],
  });

  protected readonly isEditMode = computed(() => this.product() !== null);
  protected readonly dialogTitle = computed(() => (this.isEditMode() ? 'Edit product' : 'Add product'));

  protected readonly dialogLevelError = computed(() => {
    const error = this.serverError();
    if (!error) {
      return null;
    }
    return FIELD_ERROR_CODES.includes(error.code) ? null : getErrorMessage(error.code);
  });

  constructor() {
    // (Re)populate the form whenever the dialog opens for a (possibly new) target product.
    effect(() => {
      const isOpen = this.open();
      const product = this.product();
      if (!isOpen) {
        return;
      }
      if (product) {
        this.form.reset({
          name: product.name,
          priceEuros: centsToEuroString(product.priceCents),
          quantity: product.quantity,
          imageUrl: product.imageUrl ?? '',
        });
      } else {
        this.form.reset({ name: '', priceEuros: '', quantity: 0, imageUrl: '' });
      }
    });

    // Map a server-side field error onto its control (P8-6) without touching the others.
    effect(() => {
      const error = this.serverError();
      if (!error) {
        return;
      }
      this.applyServerFieldError(error);
    });
  }

  private priceNotDuplicate(): ValidatorFn {
    return priceNotDuplicateValidator(() =>
      this.existingProducts()
        .filter((p) => p.id !== this.product()?.id)
        .map((p) => p.priceCents),
    );
  }

  private applyServerFieldError(error: ApiError): void {
    const message = getErrorMessage(error.code);
    const control = (() => {
      switch (error.code) {
        case 'DUPLICATE_PRODUCT':
          return this.form.controls.name;
        case 'DUPLICATE_PRICE':
        case 'INVALID_PRICE':
          return this.form.controls.priceEuros;
        case 'INVALID_QUANTITY':
          return this.form.controls.quantity;
        default:
          return null;
      }
    })();
    // markAsTouched so the message shows immediately, without requiring the
    // user to blur the field first — a server round trip already happened.
    control?.setErrors({ ...control.errors, server: message });
    control?.markAsTouched();
  }

  protected onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const value = this.form.getRawValue();
    const priceCents = euroStringToCents(value.priceEuros);
    if (priceCents === null) {
      return;
    }
    const imageUrl = value.imageUrl.trim();
    this.save.emit({
      name: value.name.trim(),
      priceCents,
      quantity: value.quantity,
      imageUrl: imageUrl.length > 0 ? imageUrl : null,
    });
  }

  protected onCancel(): void {
    this.closed.emit();
  }
}
