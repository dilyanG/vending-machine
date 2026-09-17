import { Component, signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Product } from '../../../core/models/product.model';
import { ProductTable } from './product-table';

@Component({
  selector: 'vm-test-host',
  imports: [ProductTable],
  template: `
    <vm-product-table
      [products]="products()"
      [busy]="busy()"
      (edit)="edited.set($event)"
      (remove)="removed.set($event)"
    />
  `,
})
class TestHost {
  readonly products = signal<Product[]>([
    {
      id: 'ee6c6e26-5faf-4144-b0d2-c94aaff69687',
      name: 'Water',
      priceCents: 85,
      quantity: 10,
      imageUrl: 'assets/products/water.svg',
    },
    {
      id: '132bc124-798d-4123-b5ce-9bba8f9d38f4',
      name: 'Espresso',
      priceCents: 120,
      quantity: 0,
      imageUrl: 'assets/products/espresso.svg',
    },
  ]);
  readonly busy = signal(false);
  readonly edited = signal<Product | null>(null);
  readonly removed = signal<Product | null>(null);
}

describe('ProductTable', () => {
  let fixture: ComponentFixture<TestHost>;

  beforeEach(() => {
    fixture = TestBed.createComponent(TestHost);
    fixture.detectChanges();
  });

  function tableRows(): HTMLTableRowElement[] {
    return Array.from(
      fixture.nativeElement.querySelectorAll('[data-testid="product-table"] tbody tr'),
    );
  }

  function cardItems(): HTMLLIElement[] {
    return Array.from(fixture.nativeElement.querySelectorAll('[data-testid="product-cards"] li'));
  }

  it('renders one table row and one card per product', () => {
    expect(tableRows().length).toBe(2);
    expect(cardItems().length).toBe(2);
  });

  it('shows the out-of-stock badge for a zero-quantity product in both layouts', () => {
    const espressoRow = tableRows()[1];
    expect(espressoRow.textContent).toContain('Out of stock');

    const espressoCard = cardItems()[1];
    expect(espressoCard.textContent).toContain('Out of stock');
  });

  it('emits edit with the clicked product', () => {
    const editButton = tableRows()[0].querySelectorAll('button')[0];
    editButton.click();
    expect(fixture.componentInstance.edited()?.name).toBe('Water');
  });

  it('emits remove with the clicked product', () => {
    const deleteButton = tableRows()[0].querySelectorAll('button')[1];
    deleteButton.click();
    expect(fixture.componentInstance.removed()?.name).toBe('Water');
  });

  it('disables action buttons while busy', () => {
    fixture.componentInstance.busy.set(true);
    fixture.detectChanges();

    const buttons = tableRows()[0].querySelectorAll('button');
    expect((buttons[0] as HTMLButtonElement).disabled).toBeTrue();
    expect((buttons[1] as HTMLButtonElement).disabled).toBeTrue();
  });
});
