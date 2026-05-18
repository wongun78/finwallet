export function formatNumber(value: number) {
  return new Intl.NumberFormat('en-US').format(value);
}

export function compactId(value: string, size = 6) {
  if (value.length <= size * 2) {
    return value;
  }
  return `${value.slice(0, size)}...${value.slice(-size)}`;
}
