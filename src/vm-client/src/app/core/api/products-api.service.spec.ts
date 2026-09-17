import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { ApiError } from './api-error';
import { errorInterceptor } from '../interceptors/error.interceptor';
import { CreateProductRequest, Product, UpdateProductRequest } from '../models/product.model';
import { ProductsApiService } from './products-api.service';

describe('ProductsApiService', () => {
  let service: ProductsApiService;
  let httpTesting: HttpTestingController;

  const product: Product = {
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
    service = TestBed.inject(ProductsApiService);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTesting.verify();
  });

  it('list() GETs /api/products and returns the typed list', () => {
    let result: Product[] | undefined;
    service.list().subscribe((res) => (result = res));

    const req = httpTesting.expectOne('/api/products');
    expect(req.request.method).toBe('GET');
    req.flush([product]);

    expect(result).toEqual([product]);
  });

  it('get(id) GETs /api/products/{id} and returns the typed product', () => {
    let result: Product | undefined;
    service.get(product.id).subscribe((res) => (result = res));

    const req = httpTesting.expectOne(`/api/products/${product.id}`);
    expect(req.request.method).toBe('GET');
    req.flush(product);

    expect(result).toEqual(product);
  });

  it('create(request) POSTs the request body to /api/products', () => {
    const request: CreateProductRequest = {
      name: 'Water',
      priceCents: 85,
      quantity: 10,
      imageUrl: 'assets/products/water.svg',
    };
    let result: Product | undefined;
    service.create(request).subscribe((res) => (result = res));

    const req = httpTesting.expectOne('/api/products');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(request);
    req.flush(product);

    expect(result).toEqual(product);
  });

  it('update(id, request) PUTs the request body to /api/products/{id}', () => {
    const request: UpdateProductRequest = {
      name: 'Water',
      priceCents: 90,
      quantity: 8,
      imageUrl: null,
    };
    let result: Product | undefined;
    service.update(product.id, request).subscribe((res) => (result = res));

    const req = httpTesting.expectOne(`/api/products/${product.id}`);
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual(request);
    req.flush({ ...product, ...request });

    expect(result).toEqual({ ...product, ...request });
  });

  it('remove(id) DELETEs /api/products/{id}', () => {
    let completed = false;
    service.remove(product.id).subscribe(() => (completed = true));

    const req = httpTesting.expectOne(`/api/products/${product.id}`);
    expect(req.request.method).toBe('DELETE');
    req.flush(null);

    expect(completed).toBe(true);
  });

  it('reload() POSTs to /api/products/reload with no body', () => {
    let completed = false;
    service.reload().subscribe(() => (completed = true));

    const req = httpTesting.expectOne('/api/products/reload');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toBeNull();
    req.flush(null);

    expect(completed).toBe(true);
  });

  it('surfaces a 409 DUPLICATE_PRODUCT as a normalised ApiError with its code intact', () => {
    let captured: ApiError | undefined;
    service.create({ name: 'Water', priceCents: 85, quantity: 10, imageUrl: null }).subscribe({
      error: (err: ApiError) => (captured = err),
    });

    httpTesting.expectOne('/api/products').flush(
      {
        code: 'DUPLICATE_PRODUCT',
        message: "A product named 'Water' already exists.",
        details: { name: 'Water' },
      },
      { status: 409, statusText: 'Conflict' },
    );

    expect(captured?.code).toBe('DUPLICATE_PRODUCT');
  });
});
