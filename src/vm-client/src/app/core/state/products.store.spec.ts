import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { errorInterceptor } from '../interceptors/error.interceptor';
import { CreateProductRequest, Product, UpdateProductRequest } from '../models/product.model';
import { ProductsStore } from './products.store';

describe('ProductsStore', () => {
  let store: ProductsStore;
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
      providers: [
        provideHttpClient(withInterceptors([errorInterceptor])),
        provideHttpClientTesting(),
      ],
    });
    store = TestBed.inject(ProductsStore);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTesting.verify();
  });

  it('load() GETs the list and populates products', () => {
    store.load();
    httpTesting.expectOne('/api/products').flush([water]);

    expect(store.products()).toEqual([water]);
    expect(store.loading()).toBe(false);
  });

  it('create() appends the returned product without refetching the list', () => {
    store.load();
    httpTesting.expectOne('/api/products').flush([water]);

    const request: CreateProductRequest = {
      name: 'Cola',
      priceCents: 145,
      quantity: 10,
      imageUrl: null,
    };
    const created: Product = { ...request, id: '96a0e81f-bdf5-4a6d-b1db-36fb755f0491' };
    let result: Product | undefined;
    store.create(request).subscribe((p) => (result = p));

    const req = httpTesting.expectOne('/api/products');
    expect(req.request.method).toBe('POST');
    req.flush(created);

    expect(result).toEqual(created);
    expect(store.products()).toEqual([water, created]);
    expect(store.busy()).toBe(false);
    // No extra GET /api/products fired — the store applied the response.
  });

  it('update() patches the matching product in place without refetching', () => {
    store.load();
    httpTesting.expectOne('/api/products').flush([water]);

    const request: UpdateProductRequest = {
      name: 'Water',
      priceCents: 90,
      quantity: 8,
      imageUrl: water.imageUrl,
    };
    const updated: Product = { ...water, ...request };
    store.update(water.id, request).subscribe();

    const req = httpTesting.expectOne(`/api/products/${water.id}`);
    expect(req.request.method).toBe('PUT');
    req.flush(updated);

    expect(store.products()).toEqual([updated]);
  });

  it('remove() filters the deleted product out without refetching', () => {
    store.load();
    httpTesting.expectOne('/api/products').flush([water]);

    store.remove(water.id).subscribe();
    const req = httpTesting.expectOne(`/api/products/${water.id}`);
    expect(req.request.method).toBe('DELETE');
    req.flush(null);

    expect(store.products()).toEqual([]);
  });

  it('reload() POSTs then re-fetches the list, since reload has no response body to apply', () => {
    store.load();
    httpTesting.expectOne('/api/products').flush([water]);

    let result: Product[] | undefined;
    store.reload().subscribe((products) => (result = products));

    const reloadReq = httpTesting.expectOne('/api/products/reload');
    expect(reloadReq.request.method).toBe('POST');
    reloadReq.flush(null);

    const listReq = httpTesting.expectOne('/api/products');
    expect(listReq.request.method).toBe('GET');
    listReq.flush([water, { ...water, id: 'new-id', name: 'Espresso', priceCents: 120 }]);

    expect(result?.length).toBe(2);
    expect(store.products().length).toBe(2);
  });

  it('ignores a second mutating call while one is already in flight', () => {
    const request: CreateProductRequest = {
      name: 'Cola',
      priceCents: 145,
      quantity: 10,
      imageUrl: null,
    };
    store.create(request).subscribe();
    store.create(request).subscribe();
    store.update(water.id, request).subscribe();
    store.remove(water.id).subscribe();

    // Only the first create() actually issued a request.
    const productsRequests = httpTesting.match('/api/products');
    expect(productsRequests.length).toBe(1);
    expect(httpTesting.match(`/api/products/${water.id}`).length).toBe(0);

    productsRequests[0].flush({ ...request, id: '96a0e81f-bdf5-4a6d-b1db-36fb755f0491' });
  });

  it('a failed create sets error and leaves products untouched', () => {
    store.load();
    httpTesting.expectOne('/api/products').flush([water]);

    let captured: unknown;
    store
      .create({ name: 'Water', priceCents: 85, quantity: 5, imageUrl: null })
      .subscribe({ error: (err) => (captured = err) });

    httpTesting.expectOne('/api/products').flush(
      {
        code: 'DUPLICATE_PRODUCT',
        message: "A product named 'Water' already exists.",
        details: { name: 'Water' },
      },
      { status: 409, statusText: 'Conflict' },
    );

    expect((captured as { code: string }).code).toBe('DUPLICATE_PRODUCT');
    expect(store.products()).toEqual([water]);
    expect(store.busy()).toBe(false);
  });
});
