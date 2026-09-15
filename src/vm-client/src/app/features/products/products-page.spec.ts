import { TestBed } from '@angular/core/testing';
import { ProductsPage } from './products-page';

describe('ProductsPage', () => {
  it('renders its title', () => {
    TestBed.configureTestingModule({ imports: [ProductsPage] });
    const fixture = TestBed.createComponent(ProductsPage);
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('h1')?.textContent).toBe('Products');
  });
});
