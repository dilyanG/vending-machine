import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { errorInterceptor } from '../interceptors/error.interceptor';
import { Product } from '../models/product.model';
import { PurchaseResult, ResetResult, VendingSession } from '../models/coin.model';
import { VendingStore } from './vending.store';

describe('VendingStore', () => {
  let store: VendingStore;
  let httpTesting: HttpTestingController;

  const water: Product = {
    id: 'ee6c6e26-5faf-4144-b0d2-c94aaff69687',
    name: 'Water',
    priceCents: 85,
    quantity: 10,
    imageUrl: 'assets/products/water.svg',
  };

  function loadWithSeed(products: Product[] = [water]): void {
    store.load();
    httpTesting.expectOne('/api/products').flush(products);
    httpTesting.expectOne('/api/vending/denominations').flush([5, 10, 20, 50, 100, 200]);
    httpTesting.expectOne('/api/vending/session').flush({
      insertedCoins: [],
      insertedTotalCents: 0,
    } satisfies VendingSession);
  }

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([errorInterceptor])),
        provideHttpClientTesting(),
      ],
    });
    store = TestBed.inject(VendingStore);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTesting.verify();
  });

  it('insertCoin accumulates the session total from the server response', () => {
    store.insertCoin(50);
    httpTesting.expectOne('/api/vending/coins').flush({
      insertedCoins: [{ denominationCents: 50, count: 1 }],
      insertedTotalCents: 50,
    } satisfies VendingSession);

    expect(store.insertedTotal()).toBe(50);

    store.insertCoin(100);
    httpTesting.expectOne('/api/vending/coins').flush({
      insertedCoins: [
        { denominationCents: 100, count: 1 },
        { denominationCents: 50, count: 1 },
      ],
      insertedTotalCents: 150,
    } satisfies VendingSession);

    expect(store.insertedTotal()).toBe(150);
  });

  it('ignores a second purchase() while one is already in flight (busy guard)', () => {
    store.purchase(water.id);
    store.purchase(water.id);
    store.purchase(water.id);

    // Exactly one request in flight, not three — proves the double-click case
    // (a Buy button clicked twice) never spends the customer's money twice.
    const requests = httpTesting.match('/api/vending/purchase');
    expect(requests.length).toBe(1);

    requests[0].flush({
      product: { ...water, quantity: 9 },
      paidCents: 85,
      priceCents: 85,
      changeCents: 0,
      changeCoins: [],
    } satisfies PurchaseResult);
  });

  it('ignores insertCoin/reset too while busy, not just purchase', () => {
    store.purchase(water.id);
    store.insertCoin(50);
    store.reset();

    expect(httpTesting.match('/api/vending/coins').length).toBe(0);
    expect(httpTesting.match('/api/vending/reset').length).toBe(0);

    httpTesting.expectOne('/api/vending/purchase').flush({
      product: { ...water, quantity: 9 },
      paidCents: 85,
      priceCents: 85,
      changeCents: 0,
      changeCoins: [],
    } satisfies PurchaseResult);
  });

  it('a successful purchase patches that product in place and clears the session', () => {
    loadWithSeed();
    store.insertCoin(100);
    httpTesting.expectOne('/api/vending/coins').flush({
      insertedCoins: [{ denominationCents: 100, count: 1 }],
      insertedTotalCents: 100,
    } satisfies VendingSession);

    store.purchase(water.id);
    httpTesting.expectOne('/api/vending/purchase').flush({
      product: { ...water, quantity: 9 },
      paidCents: 100,
      priceCents: 85,
      changeCents: 15,
      changeCoins: [{ denominationCents: 10, count: 1 }, { denominationCents: 5, count: 1 }],
    } satisfies PurchaseResult);

    expect(store.products().find((p) => p.id === water.id)?.quantity).toBe(9);
    expect(store.insertedTotal()).toBe(0);
    expect(store.lastPurchase()?.changeCents).toBe(15);
    expect(store.busy()).toBe(false);
  });

  it('a failed purchase leaves insertedTotal untouched and sets error', () => {
    loadWithSeed();
    store.insertCoin(50);
    httpTesting.expectOne('/api/vending/coins').flush({
      insertedCoins: [{ denominationCents: 50, count: 1 }],
      insertedTotalCents: 50,
    } satisfies VendingSession);

    store.purchase(water.id);
    httpTesting.expectOne('/api/vending/purchase').flush(
      {
        code: 'INSUFFICIENT_FUNDS',
        message: 'Please insert more money to buy this item.',
      },
      { status: 400, statusText: 'Bad Request' },
    );

    expect(store.insertedTotal()).toBe(50);
    expect(store.error()?.code).toBe('INSUFFICIENT_FUNDS');
    expect(store.lastPurchase()).toBeNull();
    expect(store.busy()).toBe(false);
  });

  it('reset clears the session and populates lastReturn', () => {
    store.insertCoin(200);
    httpTesting.expectOne('/api/vending/coins').flush({
      insertedCoins: [{ denominationCents: 200, count: 1 }],
      insertedTotalCents: 200,
    } satisfies VendingSession);

    store.reset();
    httpTesting.expectOne('/api/vending/reset').flush({
      returnedCoins: [{ denominationCents: 200, count: 1 }],
      returnedTotalCents: 200,
    } satisfies ResetResult);

    expect(store.insertedTotal()).toBe(0);
    expect(store.lastReturn()?.returnedTotalCents).toBe(200);
  });

  it('inserting a coin clears a previous lastPurchase and error', () => {
    loadWithSeed();
    store.insertCoin(100);
    httpTesting.expectOne('/api/vending/coins').flush({
      insertedCoins: [{ denominationCents: 100, count: 1 }],
      insertedTotalCents: 100,
    } satisfies VendingSession);
    store.purchase(water.id);
    httpTesting.expectOne('/api/vending/purchase').flush({
      product: { ...water, quantity: 9 },
      paidCents: 100,
      priceCents: 85,
      changeCents: 15,
      changeCoins: [{ denominationCents: 10, count: 1 }, { denominationCents: 5, count: 1 }],
    } satisfies PurchaseResult);
    expect(store.lastPurchase()).not.toBeNull();

    store.insertCoin(50);
    httpTesting.expectOne('/api/vending/coins').flush({
      insertedCoins: [{ denominationCents: 50, count: 1 }],
      insertedTotalCents: 50,
    } satisfies VendingSession);

    expect(store.lastPurchase()).toBeNull();
    expect(store.error()).toBeNull();
  });

  it('canAfford compares insertedTotal against the product price, not stock or eligibility', () => {
    loadWithSeed();
    expect(store.canAfford(water.id)).toBe(false);

    store.insertCoin(100);
    httpTesting.expectOne('/api/vending/coins').flush({
      insertedCoins: [{ denominationCents: 100, count: 1 }],
      insertedTotalCents: 100,
    } satisfies VendingSession);

    expect(store.canAfford(water.id)).toBe(true);
    expect(store.canAfford('unknown-id')).toBe(false);
  });
});
