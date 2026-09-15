import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'vm-vending-page',
  templateUrl: './vending-page.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class VendingPage {}
