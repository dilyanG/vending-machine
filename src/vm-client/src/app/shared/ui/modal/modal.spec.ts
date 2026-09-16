import { Component, signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Modal } from './modal';

@Component({
  selector: 'vm-test-host',
  imports: [Modal],
  template: `
    <button type="button" id="trigger" (click)="open.set(true)">Open</button>
    <vm-modal [open]="open()" (closed)="open.set(false); closedCount.set(closedCount() + 1)">
      <h2 modal-header>Title</h2>
      <p modal-body>Body text</p>
      <div modal-footer>
        <button type="button" id="footer-btn">OK</button>
      </div>
    </vm-modal>
  `,
})
class TestHost {
  readonly open = signal(false);
  readonly closedCount = signal(0);
}

describe('Modal', () => {
  let fixture: ComponentFixture<TestHost>;

  beforeEach(() => {
    fixture = TestBed.createComponent(TestHost);
    fixture.detectChanges();
  });

  afterEach(() => {
    // Leave no dialog open (and no scroll lock) for the next test.
    fixture.componentInstance.open.set(false);
    fixture.detectChanges();
    document.body.style.overflow = '';
  });

  function dialogEl(): HTMLDialogElement {
    return fixture.nativeElement.querySelector('dialog');
  }

  /**
   * HTMLDialogElement.close() fires its `close` event as a queued task, not
   * synchronously (WHATWG HTML §4.11.4), so anything driven by that event —
   * scroll unlock, `closed` emission, focus restore — needs a wait. A plain
   * setTimeout(0) is a race against that same internally-queued task with no
   * ordering guarantee; listening for the real event (registered here after
   * Angular's own `(close)` binding, so it resolves after Angular's handler
   * has run) is the only deterministic way to wait for it.
   */
  function waitForClose(dialog: HTMLDialogElement): Promise<void> {
    return new Promise((resolve) =>
      dialog.addEventListener('close', () => resolve(), { once: true }),
    );
  }

  it('does not render as open initially', () => {
    expect(dialogEl().open).toBeFalse();
  });

  it('opens the native dialog and locks body scroll when `open` becomes true', () => {
    fixture.componentInstance.open.set(true);
    fixture.detectChanges();

    expect(dialogEl().open).toBeTrue();
    expect(document.body.style.overflow).toBe('hidden');
  });

  it('projects header, body and footer content', () => {
    fixture.componentInstance.open.set(true);
    fixture.detectChanges();

    const el: HTMLElement = fixture.nativeElement;
    expect(el.querySelector('.vm-modal__header')?.textContent).toContain('Title');
    expect(el.querySelector('.vm-modal__body')?.textContent).toContain('Body text');
    expect(el.querySelector('.vm-modal__footer #footer-btn')).toBeTruthy();
  });

  it('labels the dialog by its header for screen readers', () => {
    const dialog = dialogEl();
    const headerId = dialog.getAttribute('aria-labelledby');
    expect(headerId).toBeTruthy();
    expect(fixture.nativeElement.querySelector(`#${headerId}`)?.textContent).toContain('Title');
  });

  it('unlocks scroll, emits closed and restores focus to the trigger when the dialog closes', async () => {
    const trigger: HTMLButtonElement = fixture.nativeElement.querySelector('#trigger');
    trigger.focus();

    fixture.componentInstance.open.set(true);
    fixture.detectChanges();
    expect(document.activeElement).not.toBe(trigger); // showModal() moved focus into the dialog

    const closed = waitForClose(dialogEl());
    dialogEl().close();
    await closed;
    fixture.detectChanges();

    expect(document.body.style.overflow).toBe('');
    expect(fixture.componentInstance.closedCount()).toBe(1);
    expect(document.activeElement).toBe(trigger);
  });

  it('closes when the close button is clicked', async () => {
    fixture.componentInstance.open.set(true);
    fixture.detectChanges();

    const closeBtn: HTMLButtonElement = fixture.nativeElement.querySelector('.vm-modal__close');
    const closed = waitForClose(dialogEl());
    closeBtn.click();
    expect(dialogEl().open).toBeFalse(); // the open attribute clears synchronously

    await closed;
    fixture.detectChanges();

    expect(fixture.componentInstance.closedCount()).toBe(1);
  });

  it('closes when the backdrop (the dialog element itself) is clicked', () => {
    fixture.componentInstance.open.set(true);
    fixture.detectChanges();

    dialogEl().dispatchEvent(new MouseEvent('click', { bubbles: true }));
    fixture.detectChanges();

    expect(dialogEl().open).toBeFalse();
  });

  it('does not close when a click inside the panel bubbles up', () => {
    fixture.componentInstance.open.set(true);
    fixture.detectChanges();

    const panel: HTMLElement = fixture.nativeElement.querySelector('.vm-modal__panel');
    panel.dispatchEvent(new MouseEvent('click', { bubbles: true }));
    fixture.detectChanges();

    expect(dialogEl().open).toBeTrue();
  });
});
