import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { errorInterceptor } from '../interceptors/error.interceptor';
import { CatalogueProduct } from '../models/product.model';
import { ExternalCatalogApiService } from './external-catalog-api.service';

describe('ExternalCatalogApiService', () => {
  let service: ExternalCatalogApiService;
  let httpTesting: HttpTestingController;

  const catalogueProduct: CatalogueProduct = {
    id: 'ee6c6e26-5faf-4144-b0d2-c94aaff69687',
    name: 'Water',
    priceCents: 85,
    imageUrl: 'assets/products/water.svg',
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([errorInterceptor])),
        provideHttpClientTesting(),
      ],
    });
    service = TestBed.inject(ExternalCatalogApiService);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTesting.verify();
  });

  it('catalogue() GETs /api/external/catalog and returns the typed list', () => {
    let result: CatalogueProduct[] | undefined;
    service.catalogue().subscribe((res) => (result = res));

    const req = httpTesting.expectOne('/api/external/catalog');
    expect(req.request.method).toBe('GET');
    req.flush([catalogueProduct]);

    expect(result).toEqual([catalogueProduct]);
  });
});
