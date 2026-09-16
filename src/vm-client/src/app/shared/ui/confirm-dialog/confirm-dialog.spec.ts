import { Component, signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ConfirmDialog } from './confirm-dialog';

@Component({
  selector: 'vm-test-host',
  imports: [ConfirmDialog],
  template: `
    <vm-confirm-dialog
      [open]="open()"
      title="Delete product?"
      message="This can't be undone."
      confirmLabel="Delete"
      [danger]="true"
      (confirmed)="confirmedCount.set(confirmedCount() + 1)"
      (closed)="open.set(false); closedCount.set(closedCount() + 1)"
    />
  `,
})
class TestHost {
  readonly open = signal(true);
  readonly confirmedCount = signal(0);
  readonly closedCount = signal(0);
}

function findButtonByText(root: HTMLElement, text: string): HTMLButtonElement {
  const button = Array.from(root.querySelectorAll<HTMLButtonElement>('.vm-button')).find(
    (b) => b.textContent?.trim() === text,
  );
  if (!button) {
    throw new Error(`No button with text "${text}"`);
  }
  return button;
}

describe('ConfirmDialog', () => {
  let fixture: ComponentFixture<TestHost>;

  beforeEach(() => {
    fixture = TestBed.createComponent(TestHost);
    fixture.detectChanges();
  });

  afterEach(() => {
    fixture.componentInstance.open.set(false);
    fixture.detectChanges();
    document.body.style.overflow = '';
  });

  it('renders the title and message', () => {
    const el: HTMLElement = fixture.nativeElement;
    expect(el.querySelector('.vm-modal__header')?.textContent).toContain('Delete product?');
    expect(el.querySelector('.vm-modal__body')?.textContent).toContain("This can't be undone.");
  });

  it('uses the danger button variant for confirm when danger is set', () => {
    const confirmBtn = findButtonByText(fixture.nativeElement, 'Delete');
    expect(confirmBtn.className).toContain('vm-button--danger');
  });

  it('emits confirmed then closed when the confirm button is clicked', () => {
    findButtonByText(fixture.nativeElement, 'Delete').click();

    expect(fixture.componentInstance.confirmedCount()).toBe(1);
    expect(fixture.componentInstance.closedCount()).toBe(1);
  });

  it('emits only closed, not confirmed, when cancel is clicked', () => {
    findButtonByText(fixture.nativeElement, 'Cancel').click();

    expect(fixture.componentInstance.confirmedCount()).toBe(0);
    expect(fixture.componentInstance.closedCount()).toBe(1);
  });

  it('emits closed (not confirmed) when the underlying dialog is dismissed directly', async () => {
    const dialog: HTMLDialogElement = fixture.nativeElement.querySelector('dialog');

    // HTMLDialogElement.close() fires `close` as a queued task, not
    // synchronously (WHATWG HTML §4.11.4). Wait for the real event —
    // registered after Angular's own `(close)` binding, so it resolves
    // after Modal's handler (and this component's re-emit) have run —
    // rather than racing a setTimeout against that same queued task.
    const closed = new Promise<void>((resolve) =>
      dialog.addEventListener('close', () => resolve(), { once: true }),
    );
    dialog.close();
    await closed;
    fixture.detectChanges();

    expect(fixture.componentInstance.closedCount()).toBe(1);
    expect(fixture.componentInstance.confirmedCount()).toBe(0);
  });
});
