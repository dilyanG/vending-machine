import { CentsToCurrencyPipe } from './cents-to-currency.pipe';

describe('CentsToCurrencyPipe', () => {
  let pipe: CentsToCurrencyPipe;

  beforeEach(() => {
    pipe = new CentsToCurrencyPipe();
  });

  it('formats 0 cents', () => {
    expect(pipe.transform(0)).toBe('0,00 €');
  });

  it('formats 5 cents', () => {
    expect(pipe.transform(5)).toBe('0,05 €');
  });

  it('formats 145 cents', () => {
    expect(pipe.transform(145)).toBe('1,45 €');
  });

  it('formats 200 cents', () => {
    expect(pipe.transform(200)).toBe('2,00 €');
  });

  it('formats 1999 cents', () => {
    expect(pipe.transform(1999)).toBe('19,99 €');
  });

  it('formats a large value with a thousands separator', () => {
    expect(pipe.transform(123456)).toBe('1.234,56 €');
  });

  it('returns an empty string for null', () => {
    expect(pipe.transform(null)).toBe('');
  });

  it('returns an empty string for undefined', () => {
    expect(pipe.transform(undefined)).toBe('');
  });

  it('still formats a non-integer input instead of throwing', () => {
    expect(pipe.transform(145.5)).toBe('1,46 €');
  });
});
