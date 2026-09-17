import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

/** Mirrors CLAUDE.md §2.3 — fixed here since there is no endpoint to fetch it from (unlike the coin denominations). */
export const MAX_QUANTITY_PER_PRODUCT = 15;

/** Client-side-only UX guard; the server has no name length limit. */
export const NAME_MAX_LENGTH = 60;

/**
 * Exact euro-string -> integer-cents conversion. `parseFloat('1.45') * 100`
 * is `144.99999999999997`, which truncates to 144 and silently prices things
 * a cent low — `Math.round` guards against that binary floating-point drift.
 * Returns `null` if the string isn't a plain non-negative amount with at
 * most two decimal places.
 */
export function euroStringToCents(value: string): number | null {
  const trimmed = value.trim();
  if (!/^\d+(\.\d{1,2})?$/.test(trimmed)) {
    return null;
  }
  return Math.round(parseFloat(trimmed) * 100);
}

/** The inverse of euroStringToCents, for pre-filling the form when editing. */
export function centsToEuroString(cents: number): string {
  return (cents / 100).toFixed(2);
}

export function euroPriceValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const raw = (control.value as string | null) ?? '';
    if (raw.trim().length === 0) {
      return { required: true };
    }
    const cents = euroStringToCents(raw);
    if (cents === null) {
      return { invalidFormat: true };
    }
    if (cents <= 0) {
      return { mustBePositive: true };
    }
    if (cents % 5 !== 0) {
      return { notMultipleOfFive: true };
    }
    return null;
  };
}

/**
 * Client-side convenience only (CLAUDE.md §5.3: the frontend never decides,
 * only hints) — checked against whatever product list is currently loaded.
 * The server remains the authority and still rejects a real duplicate.
 */
export function priceNotDuplicateValidator(otherPricesCents: () => number[]): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const cents = euroStringToCents((control.value as string | null) ?? '');
    if (cents === null) {
      return null;
    }
    return otherPricesCents().includes(cents) ? { duplicatePrice: true } : null;
  };
}

export function integerValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const value = control.value as number | null;
    if (value === null) {
      return null;
    }
    return Number.isInteger(value) ? null : { notInteger: true };
  };
}

export function requiredTrimmedValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const value = (control.value as string | null) ?? '';
    return value.trim().length === 0 ? { required: true } : null;
  };
}
