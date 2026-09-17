import { DenominationLabelPipe } from './denomination-label.pipe';

describe('DenominationLabelPipe', () => {
  let pipe: DenominationLabelPipe;

  beforeEach(() => {
    pipe = new DenominationLabelPipe();
  });

  it('labels sub-euro denominations in cents', () => {
    expect(pipe.transform(5)).toBe('5c');
    expect(pipe.transform(10)).toBe('10c');
    expect(pipe.transform(20)).toBe('20c');
    expect(pipe.transform(50)).toBe('50c');
  });

  it('labels euro denominations with the euro sign', () => {
    expect(pipe.transform(100)).toBe('€1');
    expect(pipe.transform(200)).toBe('€2');
  });
});
