type Listener = () => void;

const listeners = new Set<Listener>();

export const authEvents = {
  onSessionCleared(listener: Listener) {
    listeners.add(listener);
    return () => listeners.delete(listener);
  },
  emitSessionCleared() {
    listeners.forEach((listener) => listener());
  }
};
