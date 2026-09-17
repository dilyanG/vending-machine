import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { errorInterceptor } from '../../core/interceptors/error.interceptor';
import { Product } from '../../core/models/product.model';
import { ProductsPage } from './products-page';

describe('ProductsPage', () => {
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
      imports: [ProductsPage],
      providers: [
        provideHttpClient(withInterceptors([errorInterceptor])),
        provideHttpClientTesting(),
      ],
    });
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTesting.verify();
  });

  it('renders its title', () => {
    const fixture = TestBed.createComponent(ProductsPage);
    fixture.detectChanges();
    httpTesting.expectOne('/api/products').flush([water]);

    expect(fixture.nativeElement.querySelector('h1')?.textContent).toBe('Products');
  });

  it('renders the product table once loaded', () => {
    const fixture = TestBed.createComponent(ProductsPage);
    fixture.detectChanges();
    httpTesting.expectOne('/api/products').flush([water]);
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('vm-product-table')).not.toBeNull();
  });

  it('shows the empty state when there are no products', () => {
    const fixture = TestBed.createComponent(ProductsPage);
    fixture.detectChanges();
    httpTesting.expectOne('/api/products').flush([]);
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('No products yet');
  });

  it('opens the create form when Add product is clicked', () => {
    const fixture = TestBed.createComponent(ProductsPage);
    fixture.detectChanges();
    httpTesting.expectOne('/api/products').flush([water]);
    fixture.detectChanges();

    const page = fixture.componentInstance;
    expect(page['formOpen']()).toBe(false);
    page['openCreate']();
    expect(page['formOpen']()).toBe(true);
    expect(page['editingProduct']()).toBeNull();
  });

  it('opens the edit form pre-targeted at the clicked product', () => {
    const fixture = TestBed.createComponent(ProductsPage);
    fixture.detectChanges();
    httpTesting.expectOne('/api/products').flush([water]);
    fixture.detectChanges();

    const page = fixture.componentInstance;
    page['openEdit'](water);
    expect(page['formOpen']()).toBe(true);
    expect(page['editingProduct']()).toEqual(water);
  });

  it('requesting delete opens the confirm dialog naming the product', () => {
    const fixture = TestBed.createComponent(ProductsPage);
    fixture.detectChanges();
    httpTesting.expectOne('/api/products').flush([water]);
    fixture.detectChanges();

    const page = fixture.componentInstance;
    page['requestDelete'](water);
    expect(page['deleteTarget']()).toEqual(water);
    expect(page['deleteMessage']()).toContain('Water');
  });

  it('a create submission failure sets formError without closing the dialog', () => {
    const fixture = TestBed.createComponent(ProductsPage);
    fixture.detectChanges();
    httpTesting.expectOne('/api/products').flush([water]);
    fixture.detectChanges();

    const page = fixture.componentInstance;
    page['openCreate']();
    page['onSave']({ name: 'Water', priceCents: 85, quantity: 5, imageUrl: null });

    httpTesting.expectOne('/api/products').flush(
      {
        code: 'DUPLICATE_PRODUCT',
        message: "A product named 'Water' already exists.",
        details: { name: 'Water' },
      },
      { status: 409, statusText: 'Conflict' },
    );

    expect(page['formError']()?.code).toBe('DUPLICATE_PRODUCT');
    expect(page['formOpen']()).toBe(true);
  });

  it('a successful save closes the dialog and clears formError', () => {
    const fixture = TestBed.createComponent(ProductsPage);
    fixture.detectChanges();
    httpTesting.expectOne('/api/products').flush([water]);
    fixture.detectChanges();

    const page = fixture.componentInstance;
    page['openCreate']();
    page['onSave']({ name: 'Cola', priceCents: 145, quantity: 10, imageUrl: null });

    httpTesting
      .expectOne('/api/products')
      .flush({ id: 'new-id', name: 'Cola', priceCents: 145, quantity: 10, imageUrl: null });

    expect(page['formOpen']()).toBe(false);
    expect(page['formError']()).toBeNull();
  });
});
