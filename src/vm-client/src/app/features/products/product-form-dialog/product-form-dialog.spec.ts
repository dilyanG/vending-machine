import { Component, signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ApiError } from '../../../core/api/api-error';
import { CreateProductRequest, Product } from '../../../core/models/product.model';
import { ProductFormDialog } from './product-form-dialog';

@Component({
  selector: 'vm-test-host',
  imports: [ProductFormDialog],
  template: `
    <vm-product-form-dialog
      [open]="open()"
      [product]="product()"
      [existingProducts]="existingProducts()"
      [serverError]="serverError()"
      (save)="saved.set($event)"
      (closed)="closedCount.set(closedCount() + 1)"
    />
  `,
})
class TestHost {
  readonly open = signal(true);
  readonly product = signal<Product | null>(null);
  readonly existingProducts = signal<Product[]>([]);
  readonly serverError = signal<ApiError | null>(null);
  readonly saved = signal<CreateProductRequest | null>(null);
  readonly closedCount = signal(0);
}

describe('ProductFormDialog', () => {
  let fixture: ComponentFixture<TestHost>;

  const water: Product = {
    id: 'ee6c6e26-5faf-4144-b0d2-c94aaff69687',
    name: 'Water',
    priceCents: 145,
    quantity: 10,
    imageUrl: 'assets/products/water.svg',
  };

  beforeEach(() => {
    fixture = TestBed.createComponent(TestHost);
    fixture.detectChanges();
  });

  function nameInput(): HTMLInputElement {
    return fixture.nativeElement.querySelector('#product-form-name');
  }
  function priceInput(): HTMLInputElement {
    return fixture.nativeElement.querySelector('#product-form-price');
  }
  function quantityInput(): HTMLInputElement {
    return fixture.nativeElement.querySelector('#product-form-quantity');
  }
  function submitButton(): HTMLButtonElement {
    const buttons = fixture.nativeElement.querySelectorAll('[modal-footer] button');
    return buttons[1];
  }

  function setInput(el: HTMLInputElement, value: string): void {
    el.value = value;
    el.dispatchEvent(new Event('input'));
  }

  it('opens in create mode with a blank form', () => {
    expect(nameInput().value).toBe('');
    expect(priceInput().value).toBe('');
    expect(quantityInput().value).toBe('0');
  });

  it('pre-fills the form in edit mode, with the price shown as two-decimal euros', () => {
    fixture.componentInstance.product.set(water);
    fixture.detectChanges();

    expect(nameInput().value).toBe('Water');
    expect(priceInput().value).toBe('1.45');
    expect(quantityInput().value).toBe('10');
  });

  it('round-trips price unchanged: open an existing product and save without touching it', () => {
    fixture.componentInstance.product.set(water);
    fixture.detectChanges();

    submitButton().click();
    fixture.detectChanges();

    expect(fixture.componentInstance.saved()).toEqual({
      name: 'Water',
      priceCents: 145,
      quantity: 10,
      imageUrl: 'assets/products/water.svg',
    });
  });

  it('accepts quantity 0 and 15, rejects 16', () => {
    setInput(nameInput(), 'Snack');
    setInput(priceInput(), '1.00');

    setInput(quantityInput(), '0');
    fixture.detectChanges();
    submitButton().click();
    fixture.detectChanges();
    expect(fixture.componentInstance.saved()?.quantity).toBe(0);

    fixture.componentInstance.saved.set(null);
    setInput(quantityInput(), '15');
    fixture.detectChanges();
    submitButton().click();
    fixture.detectChanges();
    expect(fixture.componentInstance.saved()?.quantity).toBe(15);

    fixture.componentInstance.saved.set(null);
    setInput(quantityInput(), '16');
    fixture.detectChanges();
    submitButton().click();
    fixture.detectChanges();
    expect(fixture.componentInstance.saved()).toBeNull();
    expect(
      fixture.nativeElement.querySelector('#product-form-quantity-error')?.textContent,
    ).toContain('15');
  });

  it('a DUPLICATE_PRICE response lands on the price field and preserves the form values', () => {
    setInput(nameInput(), 'Snack');
    setInput(priceInput(), '1.45');
    setInput(quantityInput(), '5');
    fixture.detectChanges();

    fixture.componentInstance.serverError.set({
      code: 'DUPLICATE_PRICE',
      message: 'Price 145 cents is already used by another product.',
    });
    fixture.detectChanges();

    expect(priceInput().getAttribute('aria-invalid')).toBe('true');
    const errorText = fixture.nativeElement.querySelector('#product-form-price-error')?.textContent;
    expect(errorText).toContain('price');

    // The dialog never closes itself on a failed submission, and the values are untouched.
    expect(fixture.componentInstance.closedCount()).toBe(0);
    expect(nameInput().value).toBe('Snack');
    expect(priceInput().value).toBe('1.45');
    expect(quantityInput().value).toBe('5');
  });

  it('falls back to a dialog-level message for a code that does not map to a field', () => {
    fixture.componentInstance.serverError.set({ code: 'NETWORK_ERROR', message: 'offline' });
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('.vm-form__dialog-error')?.textContent).toContain(
      "can't connect",
    );
  });

  it('flags a client-side duplicate price against the loaded list, excluding the product being edited', () => {
    fixture.componentInstance.existingProducts.set([water, { ...water, id: 'other-id', priceCents: 200 }]);
    fixture.componentInstance.product.set(water);
    fixture.detectChanges();

    // Editing Water and keeping its own price (1.45) must not trigger duplicatePrice against itself.
    submitButton().click();
    fixture.detectChanges();
    expect(fixture.componentInstance.saved()).not.toBeNull();

    fixture.componentInstance.saved.set(null);
    setInput(priceInput(), '2.00');
    fixture.detectChanges();
    submitButton().click();
    fixture.detectChanges();
    expect(fixture.componentInstance.saved()).toBeNull();
    expect(
      fixture.nativeElement.querySelector('#product-form-price-error')?.textContent,
    ).toContain('Another product');
  });
});
