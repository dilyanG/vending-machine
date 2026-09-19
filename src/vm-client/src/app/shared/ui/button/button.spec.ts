import { Component, signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Button, ButtonType, ButtonVariant } from './button';

@Component({
  selector: 'vm-test-host',
  imports: [Button],
  template: `
    <vm-button
      [variant]="variant()"
      [type]="type()"
      [disabled]="disabled()"
      [loading]="loading()"
      [ariaDisabled]="ariaDisabled()"
      [ariaDescribedBy]="ariaDescribedBy()"
      (click)="clicks.set(clicks() + 1)"
    >
      Save
    </vm-button>
  `,
})
class TestHost {
  readonly variant = signal<ButtonVariant>('primary');
  readonly type = signal<ButtonType>('button');
  readonly disabled = signal(false);
  readonly loading = signal(false);
  readonly ariaDisabled = signal(false);
  readonly ariaDescribedBy = signal<string | null>(null);
  readonly clicks = signal(0);
}

describe('Button', () => {
  let fixture: ComponentFixture<TestHost>;

  beforeEach(() => {
    fixture = TestBed.createComponent(TestHost);
    fixture.detectChanges();
  });

  function buttonEl(): HTMLButtonElement {
    return fixture.nativeElement.querySelector('button');
  }

  it('renders a real <button> defaulting to type="button"', () => {
    expect(buttonEl().tagName).toBe('BUTTON');
    expect(buttonEl().getAttribute('type')).toBe('button');
  });

  it('reflects a configured type attribute', () => {
    fixture.componentInstance.type.set('submit');
    fixture.detectChanges();
    expect(buttonEl().getAttribute('type')).toBe('submit');
  });

  it('applies the variant and size classes', () => {
    fixture.componentInstance.variant.set('danger');
    fixture.detectChanges();
    expect(buttonEl().className).toContain('vm-button--danger');
    expect(buttonEl().className).toContain('vm-button--md');
  });

  it('applies the warning variant class', () => {
    fixture.componentInstance.variant.set('warning');
    fixture.detectChanges();
    expect(buttonEl().className).toContain('vm-button--warning');
  });

  it('lets a native click bubble through to the host', () => {
    buttonEl().click();
    expect(fixture.componentInstance.clicks()).toBe(1);
  });

  it('disables the native button and blocks clicks when disabled', () => {
    fixture.componentInstance.disabled.set(true);
    fixture.detectChanges();
    expect(buttonEl().disabled).toBeTrue();

    buttonEl().click();
    expect(fixture.componentInstance.clicks()).toBe(0);
  });

  it('disables the native button and marks it aria-busy while loading', () => {
    fixture.componentInstance.loading.set(true);
    fixture.detectChanges();
    expect(buttonEl().disabled).toBeTrue();
    expect(buttonEl().getAttribute('aria-busy')).toBe('true');
  });

  it('marks aria-disabled without removing the button from the tab order, and lets the caller decide whether to act on the click', () => {
    fixture.componentInstance.ariaDisabled.set(true);
    fixture.detectChanges();

    expect(buttonEl().disabled).toBeFalse();
    expect(buttonEl().getAttribute('aria-disabled')).toBe('true');

    buttonEl().click();
    expect(fixture.componentInstance.clicks()).toBe(1);
  });

  it('forwards ariaDescribedBy onto the native button', () => {
    fixture.componentInstance.ariaDescribedBy.set('reason-1');
    fixture.detectChanges();
    expect(buttonEl().getAttribute('aria-describedby')).toBe('reason-1');
  });
});
