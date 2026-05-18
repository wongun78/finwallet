import { describe, expect, it } from 'vitest';
import { compactId, formatNumber } from './format';

describe('formatNumber', () => {
  it('formats numbers with commas', () => {
    expect(formatNumber(12000)).toBe('12,000');
  });
});

describe('compactId', () => {
  it('shortens long ids', () => {
    expect(compactId('1234567890abcdef', 4)).toBe('1234...cdef');
  });
});
