import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';

export type ButtonVariant = 'primary' | 'secondary' | 'ghost' | 'danger';
export type ButtonSize = 'md' | 'lg';
export type ButtonType = 'button' | 'submit' | 'reset';

/**
 * Usage:
 *   <vm-button (click)="save()">Save</vm-button>
 *   <vm-button variant="danger" size="lg" [loading]="deleting()">Delete</vm-button>
 *   <vm-button variant="ghost" [fullWidth]="true" type="submit">Continue</vm-button>
 *
 * A real <button> underneath, so a native click on it bubbles straight
 * through this component's host — no `click` output needed.
 */
@Component({
  selector: 'vm-button',
  templateUrl: './button.html',
  styleUrl: './button.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Button {
  readonly variant = input<ButtonVariant>('primary');
  readonly size = input<ButtonSize>('md');
  readonly type = input<ButtonType>('button');
  readonly disabled = input(false);
  readonly loading = input(false);
  readonly fullWidth = input(false);

  protected readonly buttonClasses = computed(() => {
    const classes = ['vm-button', `vm-button--${this.variant()}`, `vm-button--${this.size()}`];
    if (this.fullWidth()) {
      classes.push('vm-button--full-width');
    }
    if (this.loading()) {
      classes.push('vm-button--loading');
    }
    return classes.join(' ');
  });
}
