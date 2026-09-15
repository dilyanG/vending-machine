import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { HomePage } from './home-page';

describe('HomePage', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HomePage],
      providers: [provideRouter([])],
    });
  });

  it('renders a card linking to the vending module', () => {
    const fixture = TestBed.createComponent(HomePage);
    fixture.detectChanges();
    const links = Array.from(fixture.nativeElement.querySelectorAll('a')) as HTMLAnchorElement[];
    const vendingLink = links.find((a) => a.getAttribute('href') === '/vending');
    expect(vendingLink?.textContent).toContain('Vending');
  });

  it('renders a card linking to the products module', () => {
    const fixture = TestBed.createComponent(HomePage);
    fixture.detectChanges();
    const links = Array.from(fixture.nativeElement.querySelectorAll('a')) as HTMLAnchorElement[];
    const productsLink = links.find((a) => a.getAttribute('href') === '/products');
    expect(productsLink?.textContent).toContain('Products');
  });
});
