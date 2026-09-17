import { Component, signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { PurchaseResult, ResetResult } from '../../../core/models/coin.model';
import { ChangeTray } from './change-tray';

@Component({
  selector: 'vm-test-host',
  imports: [ChangeTray],
  template: `<vm-change-tray [lastPurchase]="lastPurchase()" [lastReturn]="lastReturn()" />`,
})
class TestHost {
  readonly lastPurchase = signal<PurchaseResult | null>(null);
  readonly lastReturn = signal<ResetResult | null>(null);
}

describe('ChangeTray', () => {
  let fixture: ComponentFixture<TestHost>;

  beforeEach(() => {
    fixture = TestBed.createComponent(TestHost);
    fixture.detectChanges();
  });

  function tray(): HTMLElement {
    return fixture.nativeElement.querySelector('[data-testid="change-tray"]');
  }

  it('is empty when there is no purchase or return', () => {
    expect(tray().textContent?.trim()).toBe('');
  });

  it('shows the change breakdown unambiguously and moves focus to itself after a purchase', () => {
    fixture.componentInstance.lastPurchase.set({
      product: {
        id: 'ee6c6e26-5faf-4144-b0d2-c94aaff69687',
        name: 'Water',
        priceCents: 85,
        quantity: 9,
        imageUrl: null,
      },
      paidCents: 100,
      priceCents: 85,
      changeCents: 15,
      changeCoins: [
        { denominationCents: 10, count: 1 },
        { denominationCents: 5, count: 1 },
      ],
    });
    fixture.detectChanges();

    const items = tray().querySelectorAll('.vm-change-tray__breakdown li');
    expect(items[0].textContent?.trim()).toBe('1 x 10c');
    expect(items[1].textContent?.trim()).toBe('1 x 5c');
    expect(document.activeElement).toBe(tray());
  });

  it('shows returned coins after a reset without stealing focus', () => {
    fixture.componentInstance.lastReturn.set({
      returnedCoins: [{ denominationCents: 200, count: 1 }],
      returnedTotalCents: 200,
    });
    fixture.detectChanges();

    expect(tray().textContent).toContain('Coins returned');
    const items = tray().querySelectorAll('.vm-change-tray__breakdown li');
    expect(items[0].textContent?.trim()).toBe('1 x €2');
    expect(document.activeElement).not.toBe(tray());
  });
});
