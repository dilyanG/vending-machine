import { Component, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { Badge, BadgeVariant } from './badge';

@Component({
  selector: 'vm-test-host',
  imports: [Badge],
  template: `<vm-badge [variant]="variant()">{{ label() }}</vm-badge>`,
})
class TestHost {
  readonly variant = signal<BadgeVariant>('neutral');
  readonly label = signal('In stock');
}

describe('Badge', () => {
  it('renders projected content', () => {
    const fixture = TestBed.createComponent(TestHost);
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('.vm-badge').textContent.trim()).toBe('In stock');
  });

  it('defaults to the neutral variant', () => {
    const fixture = TestBed.createComponent(TestHost);
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('.vm-badge').className).toContain(
      'vm-badge--neutral',
    );
  });

  it('applies a configured variant class', () => {
    const fixture = TestBed.createComponent(TestHost);
    fixture.componentInstance.variant.set('danger');
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('.vm-badge').className).toContain(
      'vm-badge--danger',
    );
  });
});
