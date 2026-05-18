interface SpinnerProps {
  size?: 'sm' | 'md';
}

export function Spinner({ size = 'md' }: SpinnerProps) {
  const dimension = size === 'sm' ? 'h-4 w-4' : 'h-5 w-5';
  return <span className={`inline-block animate-spin rounded-full border-2 border-white/40 border-t-white ${dimension}`} />;
}
