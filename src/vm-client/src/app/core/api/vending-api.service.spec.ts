import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { ApiError } from './api-error';
import { errorInterceptor } from '../interceptors/error.interceptor';
import { PurchaseResult, ResetResult, VendingSession } from '../models/coin.model';
import { VendingApiService } from './vending-api.service';

describe('VendingApiService', () => {
  let service: VendingApiService;
  let httpTesting: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([errorInterceptor])),
        provideHttpClientTesting(),
      ],
    });
    service = TestBed.inject(VendingApiService);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTesting.verify();
  });

  it('denominations() GETs /api/vending/denominations and returns the typed list', () => {
    let result: number[] | undefined;
    service.denominations().subscribe((res) => (result = res));

    const req = httpTesting.expectOne('/api/vending/denominations');
    expect(req.request.method).toBe('GET');
    req.flush([5, 10, 20, 50, 100, 200]);

    expect(result).toEqual([5, 10, 20, 50, 100, 200]);
  });

  it('session() GETs /api/vending/session and returns the typed session', () => {
    const session: VendingSession = {
      insertedCoins: [{ denominationCents: 100, count: 1 }],
      insertedTotalCents: 100,
    };
    let result: VendingSession | undefined;
    service.session().subscribe((res) => (result = res));

    const req = httpTesting.expectOne('/api/vending/session');
    expect(req.request.method).toBe('GET');
    req.flush(session);

    expect(result).toEqual(session);
  });

  it('insertCoin(denominationCents) POSTs the denomination to /api/vending/coins', () => {
    const session: VendingSession = {
      insertedCoins: [{ denominationCents: 50, count: 1 }],
      insertedTotalCents: 50,
    };
    let result: VendingSession | undefined;
    service.insertCoin(50).subscribe((res) => (result = res));

    const req = httpTesting.expectOne('/api/vending/coins');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ denominationCents: 50 });
    req.flush(session);

    expect(result).toEqual(session);
  });

  it('purchase(productId) POSTs the productId to /api/vending/purchase and returns the typed result', () => {
    const productId = 'ee6c6e26-5faf-4144-b0d2-c94aaff69687';
    const purchaseResult: PurchaseResult = {
      product: {
        id: productId,
        name: 'Water',
        priceCents: 85,
        quantity: 9,
        imageUrl: 'assets/products/water.svg',
      },
      paidCents: 150,
      priceCents: 85,
      changeCents: 65,
      changeCoins: [
        { denominationCents: 50, count: 1 },
        { denominationCents: 10, count: 1 },
        { denominationCents: 5, count: 1 },
      ],
    };
    let result: PurchaseResult | undefined;
    service.purchase(productId).subscribe((res) => (result = res));

    const req = httpTesting.expectOne('/api/vending/purchase');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ productId });
    req.flush(purchaseResult);

    expect(result).toEqual(purchaseResult);
  });

  it('reset() POSTs to /api/vending/reset with no body and returns the typed result', () => {
    const resetResult: ResetResult = {
      returnedCoins: [{ denominationCents: 200, count: 1 }],
      returnedTotalCents: 200,
    };
    let result: ResetResult | undefined;
    service.reset().subscribe((res) => (result = res));

    const req = httpTesting.expectOne('/api/vending/reset');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toBeNull();
    req.flush(resetResult);

    expect(result).toEqual(resetResult);
  });

  it('surfaces a 422 CHANGE_UNAVAILABLE as a normalised ApiError with its code intact', () => {
    let captured: ApiError | undefined;
    service.purchase('ee6c6e26-5faf-4144-b0d2-c94aaff69687').subscribe({
      error: (err: ApiError) => (captured = err),
    });

    httpTesting.expectOne('/api/vending/purchase').flush(
      {
        code: 'CHANGE_UNAVAILABLE',
        message: 'The machine cannot give exact change for this purchase.',
        details: { shortfallCents: 5 },
      },
      { status: 422, statusText: 'Unprocessable Entity' },
    );

    expect(captured?.code).toBe('CHANGE_UNAVAILABLE');
    expect(captured?.details).toEqual({ shortfallCents: 5 });
  });
});
