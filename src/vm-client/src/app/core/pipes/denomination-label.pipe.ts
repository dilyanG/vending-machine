import { Pipe, PipeTransform } from '@angular/core';

/**
 * Short display label for one coin denomination, e.g. "5c", "€1", "€2" —
 * used for the coin-slot glyphs and the change-tray breakdown. Distinct
 * from `centsToCurrency`: this labels a discrete coin, not a formatted
 * money amount, so it never goes through `Intl.NumberFormat`.
 */
@Pipe({ name: 'denominationLabel' })
export class DenominationLabelPipe implements PipeTransform {
  transform(denominationCents: number): string {
    return denominationCents < 100 ? `${denominationCents}c` : `€${denominationCents / 100}`;
  }
}
