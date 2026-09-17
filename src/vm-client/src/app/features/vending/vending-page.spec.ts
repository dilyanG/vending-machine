import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { Product } from '../../core/models/product.model';
import { VendingPage } from './vending-page';

describe('VendingPage', () => {
  let httpTesting: HttpTestingController;

  const water: Product = {
    id: 'ee6c6e26-5faf-4144-b0d2-c94aaff69687',
    name: 'Water',
    priceCents: 85,
    quantity: 10,
    imageUrl: 'assets/products/water.svg',
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [VendingPage],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTesting.verify();
  });

  it('renders a loading skeleton before the initial load resolves', () => {
    const fixture = TestBed.createComponent(VendingPage);
    fixture.detectChanges();

    expect(
      fixture.nativeElement.querySelector('.vm-vending-page__skeleton'),
    ).not.toBeNull();

    httpTesting.expectOne('/api/products').flush([water]);
    httpTesting.expectOne('/api/vending/denominations').flush([5, 10, 20, 50, 100, 200]);
    httpTesting
      .expectOne('/api/vending/session')
      .flush({ insertedCoins: [], insertedTotalCents: 0 });
  });

  it('renders the product grid and coin slot once loaded', () => {
    const fixture = TestBed.createComponent(VendingPage);
    fixture.detectChanges();

    httpTesting.expectOne('/api/products').flush([water]);
    httpTesting.expectOne('/api/vending/denominations').flush([5, 10, 20, 50, 100, 200]);
    httpTesting
      .expectOne('/api/vending/session')
      .flush({ insertedCoins: [], insertedTotalCents: 0 });
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('vm-product-grid')).not.toBeNull();
    expect(fixture.nativeElement.querySelector('vm-coin-slot')).not.toBeNull();
    expect(fixture.nativeElement.querySelector('.vm-vending-page__skeleton')).toBeNull();
  });

  it('shows the empty state when the machine has no products', () => {
    const fixture = TestBed.createComponent(VendingPage);
    fixture.detectChanges();

    httpTesting.expectOne('/api/products').flush([]);
    httpTesting.expectOne('/api/vending/denominations').flush([5, 10, 20, 50, 100, 200]);
    httpTesting
      .expectOne('/api/vending/session')
      .flush({ insertedCoins: [], insertedTotalCents: 0 });
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('No products available');
  });

  it('shows a retryable error state when the initial load fails', () => {
    const fixture = TestBed.createComponent(VendingPage);
    fixture.detectChanges();

    httpTesting.expectOne('/api/products').flush('', { status: 0, statusText: 'Unknown Error' });
    // forkJoin cancels the sibling requests once one source errors, but
    // cancellation is async — drain whichever haven't been cancelled yet.
    for (const req of httpTesting.match(() => true)) {
      if (!req.cancelled) {
        req.flush([]);
      }
    }
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain("Couldn't load the machine");
  });
});
