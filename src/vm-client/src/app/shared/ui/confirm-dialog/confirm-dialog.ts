import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { Button } from '../button/button';
import { Modal } from '../modal/modal';

/**
 * Usage:
 *   <vm-confirm-dialog
 *     [open]="isConfirmOpen()"
 *     title="Delete product?"
 *     message="This can't be undone."
 *     confirmLabel="Delete"
 *     [danger]="true"
 *     (confirmed)="deleteProduct()"
 *     (closed)="isConfirmOpen.set(false)"
 *   />
 */
@Component({
  selector: 'vm-confirm-dialog',
  imports: [Modal, Button],
  templateUrl: './confirm-dialog.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ConfirmDialog {
  readonly open = input(false);
  readonly title = input('Are you sure?');
  readonly message = input.required<string>();
  readonly confirmLabel = input('Confirm');
  readonly cancelLabel = input('Cancel');
  readonly danger = input(false);

  /** Fired only when the user explicitly confirms. */
  readonly confirmed = output<void>();
  /** Fired whenever the dialog should no longer be shown, for any reason. */
  readonly closed = output<void>();

  protected onCancel(): void {
    this.closed.emit();
  }

  protected onConfirm(): void {
    this.confirmed.emit();
    this.closed.emit();
  }
}
