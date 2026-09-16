import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { EmptyState } from './empty-state';

@Component({
  selector: 'vm-test-host',
  imports: [EmptyState],
  template: `
    <vm-empty-state title="No products yet" message="Add your first product to get started.">
      <span empty-state-icon>📦</span>
      <button empty-state-action type="button">Add product</button>
    </vm-empty-state>
  `,
})
class TestHost {}

@Component({
  selector: 'vm-test-host-no-message',
  imports: [EmptyState],
  template: `<vm-empty-state title="No results" />`,
})
class TestHostNoMessage {}

describe('EmptyState', () => {
  it('renders the title and message', () => {
    const fixture = TestBed.createComponent(TestHost);
    fixture.detectChanges();
    const el: HTMLElement = fixture.nativeElement;

    expect(el.querySelector('.vm-empty-state__title')?.textContent).toContain('No products yet');
    expect(el.querySelector('.vm-empty-state__message')?.textContent).toContain(
      'Add your first product to get started.',
    );
  });

  it('projects the icon and action content', () => {
    const fixture = TestBed.createComponent(TestHost);
    fixture.detectChanges();
    const el: HTMLElement = fixture.nativeElement;

    expect(el.querySelector('.vm-empty-state__icon')?.textContent).toContain('📦');
    expect(el.querySelector('.vm-empty-state__action')?.textContent).toContain('Add product');
  });

  it('omits the message paragraph when no message is given', () => {
    const fixture = TestBed.createComponent(TestHostNoMessage);
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('.vm-empty-state__message')).toBeNull();
  });
});
