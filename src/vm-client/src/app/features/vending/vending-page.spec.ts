import { TestBed } from '@angular/core/testing';
import { VendingPage } from './vending-page';

describe('VendingPage', () => {
  it('renders its title', () => {
    TestBed.configureTestingModule({ imports: [VendingPage] });
    const fixture = TestBed.createComponent(VendingPage);
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('h1')?.textContent).toBe('Vending');
  });
});
