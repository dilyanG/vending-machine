import {
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  effect,
  input,
  output,
  viewChild,
} from '@angular/core';

let nextModalId = 0;

const FOCUSABLE_SELECTOR = [
  'a[href]',
  'button:not([disabled])',
  'input:not([disabled])',
  'select:not([disabled])',
  'textarea:not([disabled])',
  '[tabindex]:not([tabindex="-1"])',
].join(',');

/**
 * A native <dialog>-backed modal. Escape-to-close and initial focus
 * placement come from the browser's own modal-dialog behaviour
 * (showModal()). Chrome's native modal containment stops focus reaching
 * content *outside* the dialog, but it does not wrap focus at the
 * boundaries (Tab off the last element lands on <body>, not back on the
 * first), so this component adds that wrap-around itself, plus scroll
 * locking and restoring focus to whatever triggered it.
 *
 * Usage:
 *   <vm-modal [open]="isOpen()" (closed)="isOpen.set(false)">
 *     <h2 modal-header>Delete product?</h2>
 *     <p modal-body>This can't be undone.</p>
 *     <div modal-footer>
 *       <vm-button variant="ghost" (click)="isOpen.set(false)">Cancel</vm-button>
 *       <vm-button variant="danger" (click)="confirm()">Delete</vm-button>
 *     </div>
 *   </vm-modal>
 */
@Component({
  selector: 'vm-modal',
  templateUrl: './modal.html',
  styleUrl: './modal.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Modal {
  readonly open = input(false);
  readonly closed = output<void>();

  protected readonly headerId = `vm-modal-header-${nextModalId++}`;

  private readonly dialogRef = viewChild<ElementRef<HTMLDialogElement>>('dialog');
  private triggerElement: HTMLElement | null = null;
  private previousBodyOverflow = '';

  constructor() {
    effect(() => {
      const dialogEl = this.dialogRef()?.nativeElement;
      if (!dialogEl) {
        return;
      }

      if (this.open()) {
        if (!dialogEl.open) {
          this.triggerElement = document.activeElement as HTMLElement | null;
          this.previousBodyOverflow = document.body.style.overflow;
          document.body.style.overflow = 'hidden';
          dialogEl.showModal();
        }
      } else if (dialogEl.open) {
        dialogEl.close();
      }
    });
  }

  protected requestClose(): void {
    this.dialogRef()?.nativeElement.close();
  }

  protected onBackdropClick(event: MouseEvent): void {
    if (event.target === this.dialogRef()?.nativeElement) {
      this.requestClose();
    }
  }

  protected onDialogClose(): void {
    document.body.style.overflow = this.previousBodyOverflow;
    this.triggerElement?.focus();
    this.triggerElement = null;
    this.closed.emit();
  }

  protected onKeydown(event: KeyboardEvent): void {
    if (event.key !== 'Tab') {
      return;
    }

    const dialogEl = this.dialogRef()?.nativeElement;
    if (!dialogEl) {
      return;
    }

    const focusable = Array.from(dialogEl.querySelectorAll<HTMLElement>(FOCUSABLE_SELECTOR));
    if (focusable.length === 0) {
      return;
    }

    const first = focusable[0];
    const last = focusable[focusable.length - 1];

    if (event.shiftKey && document.activeElement === first) {
      event.preventDefault();
      last.focus();
    } else if (!event.shiftKey && document.activeElement === last) {
      event.preventDefault();
      first.focus();
    }
  }
}
