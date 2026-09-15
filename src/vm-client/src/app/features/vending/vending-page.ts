import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'vm-vending-page',
  template: `<h1>Vending</h1>`,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class VendingPage {}
