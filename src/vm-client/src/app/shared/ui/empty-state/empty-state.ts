import { ChangeDetectionStrategy, Component, input } from '@angular/core';

/**
 * Usage:
 *   <vm-empty-state title="No products yet" message="Add your first product to get started.">
 *     <span empty-state-icon aria-hidden="true">📦</span>
 *     <vm-button empty-state-action (click)="openCreateForm()">Add product</vm-button>
 *   </vm-empty-state>
 */
@Component({
  selector: 'vm-empty-state',
  templateUrl: './empty-state.html',
  styleUrl: './empty-state.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class EmptyState {
  readonly title = input.required<string>();
  readonly message = input<string>();
}
