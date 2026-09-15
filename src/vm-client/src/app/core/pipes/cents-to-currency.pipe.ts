import { Pipe, PipeTransform, isDevMode } from '@angular/core';

/**
 * Single source of truth for the locale used to format money. CLAUDE.md §5.3:
 * do not scatter this string across components.
 */
export const CURRENCY_LOCALE = 'de-DE';

const CURRENCY_FORMATTER = new Intl.NumberFormat(CURRENCY_LOCALE, {
  style: 'currency',
  currency: 'EUR',
});

@Pipe({
  name: 'centsToCurrency',
})
export class CentsToCurrencyPipe implements PipeTransform {
  transform(cents: number | null | undefined): string {
    if (cents === null || cents === undefined) {
      return '';
    }

    if (isDevMode() && !Number.isInteger(cents)) {
      console.warn(
        `centsToCurrency received a non-integer value: ${cents}. Cents must be whole numbers.`,
      );
    }

    return CURRENCY_FORMATTER.format(cents / 100);
  }
}
