import { Component, signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Product } from '../../../core/models/product.model';
import { ProductCard } from './product-card';

@Component({
  selector: 'vm-test-host',
  imports: [ProductCard],
  template: `
    <vm-product-card
      [product]="product()"
      [insertedTotal]="insertedTotal()"
      (buy)="bought.set($event)"
    />
  `,
})
class TestHost {
  readonly product = signal<Product>({
    id: 'ee6c6e26-5faf-4144-b0d2-c94aaff69687',
    name: 'Water',
    priceCents: 85,
    quantity: 10,
    imageUrl: 'assets/products/water.svg',
  });
  readonly insertedTotal = signal(0);
  readonly bought = signal<string | null>(null);
}

describe('ProductCard', () => {
  let fixture: ComponentFixture<TestHost>;

  beforeEach(() => {
    fixture = TestBed.createComponent(TestHost);
    fixture.detectChanges();
  });

  function buyButton(): HTMLButtonElement {
    return fixture.nativeElement.querySelector('[data-testid="product-card"] button');
  }

  function badge(): HTMLElement {
    return fixture.nativeElement.querySelector('[data-testid="product-card"] vm-badge');
  }

  it('in stock and affordable: Buy is enabled and primary, no shortfall hint', () => {
    fixture.componentInstance.insertedTotal.set(100);
    fixture.detectChanges();

    expect(badge().textContent?.trim()).toBe('10 in stock');
    expect(buyButton().disabled).toBeFalse();
    expect(buyButton().getAttribute('aria-disabled')).toBeNull();
    expect(fixture.nativeElement.querySelector('.vm-product-card__hint')).toBeNull();

    buyButton().click();
    expect(fixture.componentInstance.bought()).toBe('ee6c6e26-5faf-4144-b0d2-c94aaff69687');
  });

  it('low stock (<=3): warning badge, still buyable', () => {
    fixture.componentInstance.product.set({
      ...fixture.componentInstance.product(),
      quantity: 2,
    });
    fixture.componentInstance.insertedTotal.set(100);
    fixture.detectChanges();

    expect(badge().textContent?.trim()).toBe('Only 2 left');
    expect(buyButton().disabled).toBeFalse();
  });

  it('out of stock: danger badge, Buy is aria-disabled (not natively disabled) and does not emit on click', () => {
    fixture.componentInstance.product.set({
      ...fixture.componentInstance.product(),
      quantity: 0,
    });
    fixture.detectChanges();

    expect(badge().textContent?.trim()).toBe('Out of stock');
    expect(fixture.nativeElement.querySelector('.vm-product-card--out-of-stock')).not.toBeNull();
    expect(buyButton().disabled).toBeFalse();
    expect(buyButton().getAttribute('aria-disabled')).toBe('true');
    expect(buyButton().getAttribute('aria-describedby')).toBeTruthy();

    buyButton().click();
    expect(fixture.componentInstance.bought()).toBeNull();
  });

  it('not yet affordable: Buy stays enabled (never disabled) with a shortfall hint', () => {
    fixture.componentInstance.insertedTotal.set(50);
    fixture.detectChanges();

    expect(buyButton().disabled).toBeFalse();
    expect(buyButton().getAttribute('aria-disabled')).toBeNull();
    expect(fixture.nativeElement.querySelector('.vm-product-card__hint')?.textContent).toContain(
      '0,35',
    );

    buyButton().click();
    expect(fixture.componentInstance.bought()).toBe('ee6c6e26-5faf-4144-b0d2-c94aaff69687');
  });
});
