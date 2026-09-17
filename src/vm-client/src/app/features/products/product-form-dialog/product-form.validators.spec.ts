import { FormControl } from '@angular/forms';
import {
  centsToEuroString,
  euroPriceValidator,
  euroStringToCents,
  integerValidator,
  priceNotDuplicateValidator,
  requiredTrimmedValidator,
} from './product-form.validators';

describe('euroStringToCents', () => {
  it('converts exact euro amounts to exact cents, guarding against float drift', () => {
    // parseFloat('1.45') * 100 === 144.99999999999997 without rounding.
    expect(euroStringToCents('1.45')).toBe(145);
    expect(euroStringToCents('2.30')).toBe(230);
    expect(euroStringToCents('0.85')).toBe(85);
    expect(euroStringToCents('19.99')).toBe(1999);
    expect(euroStringToCents('0.05')).toBe(5);
  });

  it('round-trips through centsToEuroString back to the same cents', () => {
    for (const cents of [145, 230, 85, 1999, 5]) {
      const euroString = centsToEuroString(cents);
      expect(euroStringToCents(euroString)).toBe(cents);
    }
  });

  it('rejects malformed input', () => {
    expect(euroStringToCents('')).toBeNull();
    expect(euroStringToCents('abc')).toBeNull();
    expect(euroStringToCents('-1.00')).toBeNull();
    expect(euroStringToCents('1.234')).toBeNull();
  });
});

describe('centsToEuroString', () => {
  it('formats cents as a two-decimal euro string', () => {
    expect(centsToEuroString(145)).toBe('1.45');
    expect(centsToEuroString(5)).toBe('0.05');
    expect(centsToEuroString(0)).toBe('0.00');
  });
});

describe('euroPriceValidator', () => {
  const validate = (value: string) => euroPriceValidator()(new FormControl(value));

  it('rejects an amount that is not a multiple of 5 cents', () => {
    expect(validate('1.43')).toEqual({ notMultipleOfFive: true });
  });

  it('accepts a valid multiple-of-5 amount', () => {
    expect(validate('1.45')).toBeNull();
  });

  it('requires a value', () => {
    expect(validate('')).toEqual({ required: true });
    expect(validate('   ')).toEqual({ required: true });
  });

  it('rejects a non-positive amount', () => {
    expect(validate('0.00')).toEqual({ mustBePositive: true });
  });

  it('rejects an unparseable amount', () => {
    expect(validate('free')).toEqual({ invalidFormat: true });
  });
});

describe('priceNotDuplicateValidator', () => {
  it('flags a price that matches another product', () => {
    const validator = priceNotDuplicateValidator(() => [145, 230]);
    expect(validator(new FormControl('1.45'))).toEqual({ duplicatePrice: true });
  });

  it('passes a price that matches nothing', () => {
    const validator = priceNotDuplicateValidator(() => [145, 230]);
    expect(validator(new FormControl('1.75'))).toBeNull();
  });

  it('defers format errors to euroPriceValidator', () => {
    const validator = priceNotDuplicateValidator(() => [145]);
    expect(validator(new FormControl('not a price'))).toBeNull();
  });
});

describe('integerValidator', () => {
  it('accepts whole numbers, including the boundary values 0 and 15', () => {
    expect(integerValidator()(new FormControl(0))).toBeNull();
    expect(integerValidator()(new FormControl(15))).toBeNull();
  });

  it('rejects a fractional value', () => {
    expect(integerValidator()(new FormControl(5.5))).toEqual({ notInteger: true });
  });
});

describe('requiredTrimmedValidator', () => {
  it('rejects an empty or whitespace-only value', () => {
    expect(requiredTrimmedValidator()(new FormControl(''))).toEqual({ required: true });
    expect(requiredTrimmedValidator()(new FormControl('   '))).toEqual({ required: true });
  });

  it('accepts a real value', () => {
    expect(requiredTrimmedValidator()(new FormControl('Water'))).toBeNull();
  });
});
